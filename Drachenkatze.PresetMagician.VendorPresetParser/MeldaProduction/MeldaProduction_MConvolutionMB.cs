using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MConvolutionMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296909135};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandConvolutionpresets.xml", "MMultiBandConvolutionpresetspresets");
        }
    }
}