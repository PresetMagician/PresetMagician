using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRingModulatorMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299018349};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandRingModulatorpresets.xml", "MMultiBandRingModulatorpresetspresets");
        }
    }
}