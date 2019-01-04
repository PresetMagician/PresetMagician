using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_DX7: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1148729137 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"DX7"};
            ScanPresets(instruments);
        }
    }
}