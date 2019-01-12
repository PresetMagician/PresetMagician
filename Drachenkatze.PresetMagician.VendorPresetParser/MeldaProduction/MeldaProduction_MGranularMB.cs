using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MGranularMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296910194};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandGranularpresets.xml", "MMultiBandGranularpresetspresets");
        }
    }
}