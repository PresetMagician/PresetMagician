using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MBitFunMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298285126};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandBitFunpresets.xml", "MMultiBandBitFunpresets");
        }
    }
}