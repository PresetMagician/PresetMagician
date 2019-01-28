using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.Data;

namespace SharedModels
{
    public class PluginLocation: ObservableObject
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Defines the full path to the plugin DLL
        /// </summary>
        public string DllPath { get; set; }
        
        public string DllHash { get; set; }
        
        public string VendorVersion { get; set; }
        
        public string PluginVendor { get; set; }
        
        public string PluginName { get; set; }
        
        public string PluginProduct { get; set; }

        public bool IsPresent { get; set; }
        
        public int PluginId { get; set; }
    }
}