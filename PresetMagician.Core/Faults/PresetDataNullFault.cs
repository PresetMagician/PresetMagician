using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class PresetDataNullFault: GenericFault
    {
        public PresetDataNullFault()
        {
            Message = "Preset data was null";
        }
    }
}