using Common.Contracts;
using System;

namespace Server.Core
{
    public enum WarningKind { VolumeSpike, TemperatureSpikeDHT, TemperatureSpikeBMP, OutOfBandLow, OutOfBandHigh }

    public class TransferEventArgs : EventArgs
    {
        public Guid SessionId { get; private set; }
        public string Message { get; private set; }
        public TransferEventArgs(Guid id, string msg) { SessionId = id; Message = msg; }
    }

    public class SampleEventArgs : EventArgs
    {
        public Guid SessionId { get; private set; }
        public SensorSample Sample { get; private set; }
        public SampleEventArgs(Guid id, SensorSample s) { SessionId = id; Sample = s; }
    }

    public class WarningEventArgs : EventArgs
    {
        public Guid SessionId { get; private set; }
        public WarningKind Kind { get; private set; }
        public string Direction { get; private set; } // "ispod"/"iznad"
        public string Message { get; private set; }
        public WarningEventArgs(Guid id, WarningKind k, string dir, string msg)
        { SessionId = id; Kind = k; Direction = dir; Message = msg; }
    }

    public interface ITransferEvents
    {
        event EventHandler<TransferEventArgs> OnTransferStarted;
        event EventHandler<SampleEventArgs> OnSampleReceived;
        event EventHandler<TransferEventArgs> OnTransferCompleted;
        event EventHandler<WarningEventArgs> OnWarningRaised;
    }
}