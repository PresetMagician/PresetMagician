using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MXXX: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297635377, 1297635416};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MXXXpresets.xml", "Presets_MMXXXCreativepresets");
        }
    }
}