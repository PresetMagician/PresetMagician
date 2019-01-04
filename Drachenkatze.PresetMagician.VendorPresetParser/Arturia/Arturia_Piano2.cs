using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Piano2: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1349083442 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Piano V2"};
            ScanPresets(instruments);
        }
    }
}