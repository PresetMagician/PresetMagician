using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoPitch : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298232660};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MAutoPitchpresets.xml", "MAutoPitchpresetspresets");
        }
    }
}