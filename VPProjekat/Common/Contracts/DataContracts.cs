using System;
using System.Runtime.Serialization;


namespace Common.Contracts
{
    [DataContract]
    public class MetaHeader
    {
        [DataMember(IsRequired = true)] public double Volume {  get; set; }
        [DataMember(IsRequired = true)] public double T_DHT { get; set; }
        [DataMember(IsRequired = true)] public double T_BMP { get; set; }
        [DataMember(IsRequired = true)] public double Pressure { get; set; }
        [DataMember(IsRequired = true)] public DateTime DateTime { get; set; }

    }
    [DataContract]
    public class SensorSample
    {
        [DataMember(IsRequired = true)] public double Volume { get; set; }
        [DataMember(IsRequired = true)] public double T_DHT { get; set; }
        [DataMember(IsRequired = true)] public double T_BMP { get; set; }
        [DataMember(IsRequired = true)] public double Pressure { get; set; }
        [DataMember(IsRequired = true)] public DateTime DateTime { get; set; }
    }

    [DataContract]
    public class AckResponse
    {
        [DataMember] public string Message { get; set; }
        [DataMember] public Ack Ack { get; set; }
        [DataMember] public TransferStatus Status { get; set; }
    }

    [DataContract] public enum Ack { [EnumMember] ACK, [EnumMember] NACK}
    [DataContract] public enum TransferStatus { [EnumMember] IN_PROGRESS, [EnumMember] COMPLETED }

}
