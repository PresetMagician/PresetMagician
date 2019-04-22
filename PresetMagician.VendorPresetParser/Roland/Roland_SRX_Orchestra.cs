using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_SRX_Orchestra: Roland_JV1080
    {
        public override List<int> SupportedPlugins => new List<int> {1449423666};

        protected override string GetProductName()
        {
            return "SRX ORCHESTRA";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SRX_ORCHESTRA_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SRX_ORCHESTRA_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SRX_ORCHESTRA_Script;
        }
    }
    
    
}