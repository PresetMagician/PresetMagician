using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_TR909: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227073};

        protected override string GetProductName()
        {
            return "TR-909";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_TR_909_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_TR_909_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_TR_909_Script;
        }
    }
    
    
}