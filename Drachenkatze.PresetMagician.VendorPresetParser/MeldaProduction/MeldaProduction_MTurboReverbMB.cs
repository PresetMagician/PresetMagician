using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboReverbMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299468909};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MTurboReverbMBpresets.xml", "MTurboReverbMBpresets");
        }
    }
}