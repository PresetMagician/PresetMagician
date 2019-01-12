using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MUltraMaximizer: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296133496};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MUltraMaximizerpresets.xml", "MUltraMaximizerpresetspresets");
        }
    }
}