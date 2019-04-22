using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_TB303: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227075};

        protected override string GetProductName()
        {
            return "TB-303";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_TB_303_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_TB_303_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_TB_303_Script;
        }
    }
    
    
}