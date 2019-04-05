using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_CompFET76 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179924278};

        public override int AudioPreviewPreDelay { get; set; } = 2048;

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Comp FET-76"};
        }
    }
}