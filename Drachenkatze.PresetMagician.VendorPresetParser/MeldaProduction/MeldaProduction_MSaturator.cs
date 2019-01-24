using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSaturator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131380};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MSaturatorpresets.xml", "MSaturatorpresetspresets");
        }
    }
}