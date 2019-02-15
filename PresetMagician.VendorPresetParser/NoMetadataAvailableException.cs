using System;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NoMetadataAvailableException: Exception
    {
        public NoMetadataAvailableException(IRemotePluginInstance pluginInstance): base($"{pluginInstance.Plugin.DllPath} has no metadata loaded, unable to continue")
        {
        }
    }
}