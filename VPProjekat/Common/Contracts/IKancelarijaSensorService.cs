using System;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Common.Contracts
{
    [ServiceContract]
    public interface IKancelarijaSensorService
    {
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]

        AckResponse StartSession(MetaHeader meta);

        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]

        AckResponse PushSample(SensorSample sample);
        [OperationContract]

        AckResponse EndSession();
    }
}
