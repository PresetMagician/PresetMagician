using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDrumLeveler : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296331340};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MDrumLevelerpresets.xml", "MDrumLevelerpresets");
        }
    }
}