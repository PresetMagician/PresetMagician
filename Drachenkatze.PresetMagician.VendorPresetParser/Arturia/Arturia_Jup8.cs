using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Jup8: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1247105075 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Jup-8"};
            ScanPresets(instruments);
        }
    }
}