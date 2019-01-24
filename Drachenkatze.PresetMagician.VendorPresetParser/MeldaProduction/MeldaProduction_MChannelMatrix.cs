using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MChannelMatrix : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296263245};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MChannelMatrixpresets.xml", "MChannelMatrixpresets");
        }
    }
}