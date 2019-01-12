using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MVibratoMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011177};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandVibratopresets.xml", "MMultiBandVibratopresetspresets");
        }
    }
}