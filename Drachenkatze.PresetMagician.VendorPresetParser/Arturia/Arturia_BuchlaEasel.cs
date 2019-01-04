using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_BuchlaEasel: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1114981729 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Buchla Easel"};
            ScanPresets(instruments);
        }
    }
}