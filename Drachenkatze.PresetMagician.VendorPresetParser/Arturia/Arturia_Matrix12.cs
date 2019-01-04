using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Matrix12: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298232370 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Matrix-12"};
            ScanPresets(instruments);
        }
    }
}