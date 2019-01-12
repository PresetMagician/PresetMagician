using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoSpread: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297314899};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MStereoSpreadpresets.xml", "MStereoSpreadpresetspresets");
        }
    }
}