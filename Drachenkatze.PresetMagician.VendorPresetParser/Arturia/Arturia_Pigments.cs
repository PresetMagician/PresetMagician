using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Pigments : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1264677937};

        public override int AudioPreviewPreDelay { get; set; } = 2048;

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Pigments"};
        }
    }
}