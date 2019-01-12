using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboCompMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297367917};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MTurboCompMBpresets.xml", "MTurboCompMBpresets");
        }
    }
}