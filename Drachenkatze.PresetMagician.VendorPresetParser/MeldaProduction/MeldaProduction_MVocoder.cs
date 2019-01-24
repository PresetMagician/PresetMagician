using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MVocoder : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297510243};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MVocoderpresets.xml", "MVocoderpresetspresets");
        }
    }
}