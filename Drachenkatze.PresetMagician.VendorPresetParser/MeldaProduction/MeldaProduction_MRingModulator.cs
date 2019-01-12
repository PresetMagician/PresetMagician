using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRingModulator: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131382};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MRingModulatorpresets.xml", "MRingModulatorpresets");
        }
    }
}