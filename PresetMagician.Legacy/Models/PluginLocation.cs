using System;
using System.ComponentModel.DataAnnotations;

namespace PresetMagician.Legacy.Models
{
    public class PluginLocation 
    {
        [Key] public int Id { get; set; }

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

        public int VstPluginId { get; set; }

       
    }
}