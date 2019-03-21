using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class PluginNotRegisteredFault : GenericFault
    {
        public PluginNotRegisteredFault()
        {
            Message = "Plugin not registered with host process";
        }
    }
}