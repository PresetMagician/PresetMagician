using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class AccessViolationFault: GenericFault
    {
        public AccessViolationFault()
        {
            Message = "Access Violation (Plugin crashed)";
        }
        
    }
}