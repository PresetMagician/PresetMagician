using System;
using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class GenericFault: IGenericFault
    {
        [DataMember] public string Message { get; set; }
        [DataMember] public Exception InnerException { get; set; }
    }

    public interface IGenericFault
    {
        [DataMember] string Message { get; set; }
        [DataMember] Exception InnerException { get; set; }
    }
}