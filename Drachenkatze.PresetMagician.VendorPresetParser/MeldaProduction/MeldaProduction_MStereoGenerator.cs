using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoGenerator: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297314887};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MStereoGeneratorpresets.xml", "MStereoGeneratorpresetspresets");
        }
    }
}