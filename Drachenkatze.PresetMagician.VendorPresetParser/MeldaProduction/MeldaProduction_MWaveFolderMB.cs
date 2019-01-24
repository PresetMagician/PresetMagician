using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWaveFolderMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011430};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandWaveFolderpresets.xml", "MMultiBandWaveFolderpresets");
        }
    }
}