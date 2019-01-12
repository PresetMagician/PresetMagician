using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSpectralDelay: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297313892};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MSpectralDelaypresets.xml", "MSpectralDelaypresets");
        }
    }
}