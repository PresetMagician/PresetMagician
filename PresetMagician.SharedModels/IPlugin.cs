using System;
using System.Collections.Generic;
using Catel.Logging;
using PresetMagician.Models;

namespace SharedModels
{
    public interface IPlugin
    {
        string DllHash { get; set; }

        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        string DllPath { get; set; }

        Plugin.PluginTypes PluginType { get; set; }
        int PluginId { get; set; }
        string PluginName { get; set; }
        string PluginVendor { get; set; }
        VstPluginInfoSurrogate PluginInfo { get; set; }

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        List<PluginInfoItem> PluginInfoItems { get; }

        IVendorPresetParser PresetParser { get; set; }

        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        bool IsSupported { get; set; }

        ILog Logger { get; }

        void OnLoadError(Exception e);
    }
}