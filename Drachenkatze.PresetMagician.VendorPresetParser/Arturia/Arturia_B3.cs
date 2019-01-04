using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_B3: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1416588887 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"B-3"};
            ScanPresets(instruments);
        }
    }
}