using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_TridAPre: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1414676818 };

        public void ScanBanks()
        {
            var instruments = new List<string> {"TridA-Pre"};
            ScanPresets(instruments);
        }
    }
}