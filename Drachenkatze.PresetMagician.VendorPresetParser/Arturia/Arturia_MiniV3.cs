using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_MiniV3: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1296649779 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Mini"};
            ScanPresets(instruments);
        }
    }
}