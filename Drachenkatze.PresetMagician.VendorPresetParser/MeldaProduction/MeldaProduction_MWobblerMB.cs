using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWobblerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296914287};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MWobblerMBpresets.xml", "MWobblerMBpresets");
        }
    }
}