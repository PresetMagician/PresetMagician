using System;
using PresetMagician.VstHost.VST;

namespace PresetMagician.ProcessIsolation.Exceptions
{
    public class VstPluginException : Exception
    {
        public VstPluginException(RemoteVstPlugin plugin, string message) : base(FormatMessage(plugin, message))
        {
        }

        private static string FormatMessage(RemoteVstPlugin plugin, string message)
        {
            return plugin.DllFilename + ": " + message;
        }
    }

    public class VstPluginNotLoadedException : VstPluginException
    {
        public VstPluginNotLoadedException(RemoteVstPlugin plugin) : base(plugin, "A plugin operation was attempted, but the plugin is not loaded")
        {
        }
    }
    
    public class VstPluginEditorNotOpenException : VstPluginException
    {
        public VstPluginEditorNotOpenException(RemoteVstPlugin plugin) : base(plugin, "An attempt to create a screenshot was made, but the plugin editor is not open.")
        {
        }
    }
}