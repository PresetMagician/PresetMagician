using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_MiniFilter: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298744937 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Mini-Filter"};
            ScanPresets(instruments);
        }
    }
}