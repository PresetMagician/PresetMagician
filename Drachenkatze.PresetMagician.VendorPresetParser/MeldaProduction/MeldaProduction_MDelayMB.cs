using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDelayMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1735931701};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandDelaypresets.xml", "MMultiBandDelaypresetspresets");
        }
    }
}