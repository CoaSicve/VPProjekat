using System.Runtime.Serialization;

namespace Common.Contracts
{
    [DataContract]
    public class DataFormatFault
    {
        [DataMember] public string Reason { get; set; }
    }
    [DataContract]

    public class ValidationFault
    {
        [DataMember] public string Reason { get; set; }
        [DataMember] public string Field { get; set; }
        [DataMember] public string Value { get; set; }

    }
}
