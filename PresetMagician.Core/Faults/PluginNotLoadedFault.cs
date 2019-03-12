using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class PluginNotLoadedFault: GenericFault
    {
        public PluginNotLoadedFault()
        {
            Message = "Plugin not loaded";
        }
    }
}