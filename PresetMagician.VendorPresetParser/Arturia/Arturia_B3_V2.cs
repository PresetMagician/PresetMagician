using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_B3_V2 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416517426};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"B-3 V2"};
        }
    }
}