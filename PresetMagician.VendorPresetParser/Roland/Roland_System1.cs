using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_System1: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227060};

        protected override string GetProductName()
        {
            return "SYSTEM-1";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SYSTEM_1_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SYSTEM_1_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SYSTEM_1_Script;
        }
    }
    
    
}