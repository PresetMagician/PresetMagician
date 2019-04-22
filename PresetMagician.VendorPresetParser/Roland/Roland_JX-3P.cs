using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_JX3P: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227074};

        protected override string GetProductName()
        {
            return "JX-3P";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_JX_3P_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_JX_3P_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_JX_3P_Script;
        }
    }
    
    
}