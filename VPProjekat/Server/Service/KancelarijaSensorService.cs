using System;
using System.ServiceModel;
using Common.Contracts;

namespace Server.Service
{
    public class KancelarijaSensorService : IKancelarijaSensorService
    {
        public AckResponse StartSession(MetaHeader meta) 
        {
            return new AckResponse { };
        }
        public AckResponse PushSample(SensorSample sample) 
        {
            return new AckResponse { };
        }
        public AckResponse EndSession() 
        {
            return new AckResponse { };
        }
        
        private static void Require(bool c, string f, string v, string m) 
        {
            if (!c) throw new FaultException<ValidationFault>(new ValidationFault { Field = f, Value = v, Reason = m }, m);
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
    }
}
