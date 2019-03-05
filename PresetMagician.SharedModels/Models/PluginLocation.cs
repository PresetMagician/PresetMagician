using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Catel.Data;
using SharedModels.Collections;
using TrackableEntities.Client;

namespace SharedModels
{
    public class PluginLocation: TrackableModelBase
    {
        [Key]
        public int Id { get; set; }
        
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

        public bool IsPresent { get; set; }
        
        public int VstPluginId { get; set; }        

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