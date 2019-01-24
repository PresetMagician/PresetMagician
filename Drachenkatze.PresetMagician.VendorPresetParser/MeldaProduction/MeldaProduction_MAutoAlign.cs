using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoAlign : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296122209};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MAutoAlignpresets.xml", "MAutoAlignpresetspresets");
        }
    }
}