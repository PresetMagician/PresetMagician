using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPowerSynth: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297380722};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MSynthesizerpresets.xml", "Presets_MSynthesizerpresets");
        }
    }
}