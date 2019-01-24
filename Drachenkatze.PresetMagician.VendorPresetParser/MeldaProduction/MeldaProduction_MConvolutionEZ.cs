using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MConvolutionEZ : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296262522};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MConvolutionEZpresets.xml", "MConvolutionEZpresets");
        }
    }
}