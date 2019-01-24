using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MLoudnessAnalyzer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296851310};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MLoudnessAnalyzerpresets.xml", "MLoudnessAnalyzerpresetspresets");
        }
    }
}