using System;
using Ceras;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public class PluginLocation : ModelBase
    {
        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        [Include] public string DllPath { get; set; }

        [Include] public string DllHash { get; set; }

        [Include] public DateTime LastModifiedDateTime { get; set; }

        [Include] public string VendorVersion { get; set; }

        [Include] public string PluginVendor { get; set; }

        [Include] public string PluginName { get; set; }

        [Include] public string PluginProduct { get; set; }

        public bool IsPresent { get; set; }

        [Include] public int VstPluginId { get; set; }

        public string ShortTextRepresentation
        {
            get { return $"{PluginName} by {PluginVendor}, Version {VendorVersion}"; }
        }

        public string FullTextRepresentation
        {
            get { return $"{PluginName} by {PluginVendor}, Version {VendorVersion} ({DllPath}"; }
        }
    }
}