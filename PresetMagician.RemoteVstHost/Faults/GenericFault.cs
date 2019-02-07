using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class GenericFault
    {
        [DataMember] public bool Result { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string Description { get; set; }
    }
}