using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MLimiterMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131369};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandLimiterpresets.xml", "MMultiBandLimiterpresetspresets");
        }
    }
}