using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_V76Pre: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1413827653 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"V76-Pre"};
            ScanPresets(instruments);
        }
    }
}