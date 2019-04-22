using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_SH2: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227057};

        protected override string GetProductName()
        {
            return "SH-2";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SH_2_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SH_2_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SH_2_Script;
        }
    }
    
    
}