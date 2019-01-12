using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MBandPass: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296195904};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MBandPasspresets.xml", "MBandPasspresetspresets");
        }
    }
}