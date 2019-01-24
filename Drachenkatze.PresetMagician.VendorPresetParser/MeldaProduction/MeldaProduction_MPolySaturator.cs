using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPolySaturator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297117011};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MPolySaturatorpresets.xml", "MPolySaturatorpresets");
        }
    }
}