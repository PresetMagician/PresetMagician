using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_VoxContinental: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1450145842 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"VOX Continental"};
            ScanPresets(instruments);
        }
    }
}