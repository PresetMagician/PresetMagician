using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Farfisa: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1180791398 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"Farfisa"};
            ScanPresets(instruments);
        }
    }
}