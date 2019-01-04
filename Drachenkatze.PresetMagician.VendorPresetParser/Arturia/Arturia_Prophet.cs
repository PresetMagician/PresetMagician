using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Prophet: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1347571507 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Prophet"};
            ScanPresets(instruments);
        }
    }
}