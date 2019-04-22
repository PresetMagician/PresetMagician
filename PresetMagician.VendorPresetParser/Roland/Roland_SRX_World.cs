using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_SRX_World: Roland_JV1080
    {
        public override List<int> SupportedPlugins => new List<int> {1449423670};

        protected override string GetProductName()
        {
            return "SRX WORLD";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SRX_WORLD_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SRX_WORLD_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SRX_WORLD_Script;
        }
    }
}