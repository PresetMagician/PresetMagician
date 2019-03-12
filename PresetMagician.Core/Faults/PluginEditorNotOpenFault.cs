using System.Runtime.Serialization;

namespace PresetMagician.RemoteVstHost.Faults
{
    [DataContract]
    public class PluginEditorNotOpenFault: GenericFault
    {
        public PluginEditorNotOpenFault()
        {
            Message = "Attempted to do a plugin editor operation, but plugin editor not open";
        }
    }
}