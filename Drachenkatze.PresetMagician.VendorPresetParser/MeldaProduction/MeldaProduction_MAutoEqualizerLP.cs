using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoEqualizerLP: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296137778};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MAutoEqualizerLinearPhasepresets.xml", "presets");
        }
    }
}