using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_System100: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227059};

        protected override string GetProductName()
        {
            return "SYSTEM-100";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SYSTEM_100_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SYSTEM_100_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SYSTEM_100_Script;
        }
    }
    
    
}