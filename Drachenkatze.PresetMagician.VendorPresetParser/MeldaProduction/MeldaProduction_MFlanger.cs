using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFlanger : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296123174};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MFlangerpresets.xml", "MFlangerpresetspresets");
        }
    }
}