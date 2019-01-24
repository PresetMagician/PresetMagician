using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFreqShifterMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296125254};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandFreqShifterpresets.xml", "MMultiBandFreqShifterpresetspresets");
        }
    }
}