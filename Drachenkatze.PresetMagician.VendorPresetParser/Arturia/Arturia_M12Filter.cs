using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_M12Filter: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298220649 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"M12-Filter"};
            ScanPresets(instruments);
        }
    }
}