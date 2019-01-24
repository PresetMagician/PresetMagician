using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRhythmizerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297246241};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandRhythmizerpresets.xml", "MMultiBandRhythmizerpresetspresets");
        }
    }
}