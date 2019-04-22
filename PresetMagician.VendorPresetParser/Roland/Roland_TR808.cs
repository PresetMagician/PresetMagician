using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_TR808: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227065};

        protected override string GetProductName()
        {
            return "TR-808";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_TR_808_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_TR_808_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_TR_808_Script;
        }
    }
    
    
}