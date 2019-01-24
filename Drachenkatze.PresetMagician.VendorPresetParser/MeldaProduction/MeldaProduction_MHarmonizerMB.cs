using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MHarmonizerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299018344};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandHarmonizerpresets.xml", "MMultiBandHarmonizerpresetspresets");
        }
    }
}