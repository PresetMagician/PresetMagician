using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_CompTubeSTA : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1414747201};

        public override int AudioPreviewPreDelay { get; set; } = 2048;

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Comp TUBE-STA"};
        }
    }
}