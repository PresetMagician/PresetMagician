using System;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Exceptions
{
    public class NoMetadataAvailableException : Exception
    {
        public NoMetadataAvailableException(IRemotePluginInstance pluginInstance) : base(
            $"{pluginInstance.Plugin.DllPath} has no metadata loaded, unable to continue")
        {
        }
    }
}