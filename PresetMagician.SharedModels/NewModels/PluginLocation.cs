using System;
using System.ComponentModel.DataAnnotations;
using Ceras;
using SharedModels.Data;

namespace SharedModels.NewModels
{
    public class PluginLocation : ModelBase
    {
        [Key] [Exclude] public int Id { get; set; }

        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        public string DllPath { get; set; }

        public string DllHash { get; set; }

        public DateTime LastModifiedDateTime { get; set; }

        public string VendorVersion { get; set; }

        public string PluginVendor { get; set; }

        public string PluginName { get; set; }

        public string PluginProduct { get; set; }

        [Exclude] public bool IsPresent { get; set; }

        public int VstPluginId { get; set; }

        [Exclude]
        public string ShortTextRepresentation
        {
            get { return $"{PluginName} by {PluginVendor}, Version {VendorVersion}"; }
        }

        [Exclude]
        public string FullTextRepresentation
        {
            get { return $"{PluginName} by {PluginVendor}, Version {VendorVersion} ({DllPath}"; }
        }
    }
}