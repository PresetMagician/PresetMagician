using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWaveShaperMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011443};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandWaveShaperpresets.xml", "MMultiBandWaveShaperpresetspresets");
        }
    }
}