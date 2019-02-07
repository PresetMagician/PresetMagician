using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class NoEntryPointFoundFault: GenericFault
    {
        public NoEntryPointFoundFault()
        {
            Message = "Probably not a VST plugin";
        }
    }
}