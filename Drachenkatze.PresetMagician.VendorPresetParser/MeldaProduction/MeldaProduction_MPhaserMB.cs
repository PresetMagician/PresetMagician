using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPhaserMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {825308471};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandPhaserpresets.xml", "MMultiBandPhaserpresetspresets");
        }
    }
}