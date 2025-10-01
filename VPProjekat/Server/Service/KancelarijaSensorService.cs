using Common.Contracts;
using Server.Core;
using System;
using System.ServiceModel;

namespace Server.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class KancelarijaSensorService : IKancelarijaSensorService, ITransferEvents, IDisposable
    {
        private readonly Thresholds _thr = new Thresholds();
        private readonly string _rootPath;
        private SessionContext _ctx;
        private bool _inProgress;

        public event EventHandler<TransferEventArgs> OnTransferStarted;
        public event EventHandler<SampleEventArgs> OnSampleReceived;
        public event EventHandler<TransferEventArgs> OnTransferCompleted;
        public event EventHandler<WarningEventArgs> OnWarningRaised;

        public KancelarijaSensorService(string rootPath) { _rootPath = rootPath; }

        public AckResponse StartSession(MetaHeader meta)
        {
            if (_inProgress) return new AckResponse { Ack = Ack.NACK, Status = TransferStatus.IN_PROGRESS, Message = "Sesija već u toku." };
            ValidateMeta(meta);

            var tag = "session_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N");
            _ctx = new SessionContext(new FileStorage(_rootPath, tag), new AnalyticsEngine(_thr));
            _ctx.Analytics.Reset();

            _inProgress = true;
            if (OnTransferStarted != null) OnTransferStarted(this, new TransferEventArgs(_ctx.SessionId, "Prenos u toku..."));

            _ctx.Storage.Append(new SensorSample
            {
                Volume = meta.Volume,
                T_DHT = meta.T_DHT,
                T_BMP = meta.T_BMP,
                Pressure = meta.Pressure,
                DateTime = meta.DateTime
            });

            return new AckResponse { Ack = Ack.ACK, Status = TransferStatus.IN_PROGRESS, Message = "StartSession OK" };
        }

        public AckResponse PushSample(SensorSample s)
        {
            if (!_inProgress) return new AckResponse { Ack = Ack.NACK, Status = TransferStatus.COMPLETED, Message = "Nema aktivne sesije." };

            try { ValidateSample(s); }
            catch (FaultException fe) { _ctx.Storage.Reject(fe.Message, s); return new AckResponse { Ack = Ack.NACK, Status = TransferStatus.IN_PROGRESS, Message = fe.Message }; }

            _ctx.Storage.Append(s);
            if (OnSampleReceived != null) OnSampleReceived(this, new SampleEventArgs(_ctx.SessionId, s));

            var res = _ctx.Analytics.Process(s);
            bool v = res.Item1, dht = res.Item3, bmp = res.Item5, oob = res.Item7;
            string dv = res.Item2, dd = res.Item4, db = res.Item6, ob = res.Rest.Item1;

            if (v && OnWarningRaised != null) OnWarningRaised(this, new WarningEventArgs(_ctx.SessionId, WarningKind.VolumeSpike, dv, "ΔV > V_threshold (" + _thr.VThreshold + ")"));
            if (dht && OnWarningRaised != null) OnWarningRaised(this, new WarningEventArgs(_ctx.SessionId, WarningKind.TemperatureSpikeDHT, dd, "ΔTdht > T_dht_threshold (" + _thr.TDhtThreshold + ")"));
            if (bmp && OnWarningRaised != null) OnWarningRaised(this, new WarningEventArgs(_ctx.SessionId, WarningKind.TemperatureSpikeBMP, db, "ΔTbmp > T_bmp_threshold (" + _thr.TBmpThreshold + ")"));
            if (oob && OnWarningRaised != null) OnWarningRaised(this, new WarningEventArgs(_ctx.SessionId, ob == "iznad" ? WarningKind.OutOfBandHigh : WarningKind.OutOfBandLow, ob, "Volume van ±" + (_thr.OutOfBandPercent * 100).ToString("0") + "% srednje vrednosti"));

            return new AckResponse { Ack = Ack.ACK, Status = TransferStatus.IN_PROGRESS, Message = "Sample OK" };
        }

        public AckResponse EndSession()
        {
            if (!_inProgress) return new AckResponse { Ack = Ack.NACK, Status = TransferStatus.COMPLETED, Message = "Sesija nije aktivna." };

            _inProgress = false;
            if (OnTransferCompleted != null) OnTransferCompleted(this, new TransferEventArgs(_ctx.SessionId, "Završen prenos."));
            _ctx.Dispose(); _ctx = null;

            return new AckResponse { Ack = Ack.ACK, Status = TransferStatus.COMPLETED, Message = "EndSession OK" };
        }
        private static void Require(bool cond, string field, string value, string msg)
        {
            if (!cond) throw new FaultException<ValidationFault>(new ValidationFault { Field = field, Value = value, Reason = msg }, msg);
        }
        private static void ValidateMeta(MetaHeader m)
        {
            if (m == null) throw new FaultException<DataFormatFault>(new DataFormatFault { Reason = "MetaHeader je null/nepostojeci." }, "MetaHeader je null/nepostojeci.");
            Require(!double.IsNaN(m.Volume), nameof(m.Volume), m.Volume.ToString(), "Volume nije broj.");
            Require(!double.IsNaN(m.T_DHT), nameof(m.T_DHT), m.T_DHT.ToString(), "T_DHT nije broj.");
            Require(!double.IsNaN(m.T_BMP), nameof(m.T_BMP), m.T_BMP.ToString(), "T_BMP nije broj.");
            Require(m.Pressure > 0, nameof(m.Pressure), m.Pressure.ToString(), "Pressure mora biti veci od 0.");
        }

        private static void ValidateSample(SensorSample s)
        {
            if (s == null) throw new FaultException<DataFormatFault>(new DataFormatFault { Reason = "Sample je null/nepostojeci." }, "Sample je null/nepostojeci.");
            Require(!double.IsNaN(s.Volume) && s.Volume >= 0, nameof(s.Volume), s.Volume.ToString(), "Volume nije broj ili je manji od nule.");
            Require(!double.IsNaN(s.T_DHT), nameof(s.T_DHT), s.T_DHT.ToString(), "T_DHT nije broj.");
            Require(!double.IsNaN(s.T_BMP), nameof(s.T_BMP), s.T_BMP.ToString(), "T_BMP nije broj.");
            Require(s.Pressure > 0, nameof(s.Pressure), s.Pressure.ToString(), "Pressure mora biti veci od 0.");
        }

        public void Dispose() { if (_ctx != null) _ctx.Dispose(); }
    }
}
