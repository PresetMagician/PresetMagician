using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_Jupiter8: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227062};

        protected override string GetProductName()
        {
            return "JUPITER-8";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_JUPITER_8_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_JUPITER_8_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_JUPITER_8_Script;
        }
    }
    
    
}