using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Modular: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1297040435 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Modular"};
            ScanPresets(instruments);
        }
    }
}