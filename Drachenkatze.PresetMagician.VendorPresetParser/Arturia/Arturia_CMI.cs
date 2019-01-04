using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_CMI: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1129138550 };

        public override string Remarks { get; set; } =
            "Some audio previews are exported empty; however, that should be less than 1% of all presets.";
        
        public override int AudioPreviewPreDelay { get; set; } = 4096;
        
        public void ScanBanks()
        {
            var instruments = new List<string> {"CMI"};
            ScanPresets(instruments);
        }
    }
}