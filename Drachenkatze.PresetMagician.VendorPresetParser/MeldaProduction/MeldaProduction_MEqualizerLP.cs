using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MEqualizerLP: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131434};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MEqualizerLinearPhasepresets.xml", "MEqualizerpresets");
        }
    }
}