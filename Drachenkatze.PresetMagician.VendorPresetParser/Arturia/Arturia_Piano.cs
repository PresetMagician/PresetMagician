using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Piano: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1349083502 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Piano"};
            ScanPresets(instruments);
        }
    }
}