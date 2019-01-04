using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Synclavier: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1400464236 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Synclavier"};
            ScanPresets(instruments);
        }
    }
}