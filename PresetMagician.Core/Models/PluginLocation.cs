using System;
using System.Collections.Generic;
using Ceras;
using PresetMagician.Core.Data;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Models
{
    public class PluginLocation : ModelBase
    {
        private static HashSet<string> _editableProperties = new HashSet<string>();
        
        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

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
        [Include] public bool HasMetadata { get; set; }

        /// <summary>
        /// Defines the PresetMagician version in which the analysis failed.
        /// </summary>
        [Include]
        public string LastFailedAnalysisVersion { get; set; }
        
        public string ShortTextRepresentation => $"{PluginName} by {PluginVendor}, Version {VendorVersion}";

        public string FullTextRepresentation => $"{PluginName} by {PluginVendor}, Version {VendorVersion}, Preset Parser {PresetParser?.PresetParserType} ({DllPath})";
        
        public string GetSavedPresetParserClassName()
        {
            return _presetParserClassName;
        }
        
        private string _presetParserClassName;
        
        [Include] public string PresetParserClassName
        {
            get => PresetParser?.PresetParserType;

            set => _presetParserClassName = value;
        }
        
        private IVendorPresetParser _presetParser;

        public IVendorPresetParser PresetParser
        {
            get { return _presetParser; }
            set
            {
                if (value != null)
                {
                    _presetParserClassName = null;
                }

                _presetParser = value;
            }
        }
    }
}