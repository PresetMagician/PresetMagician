using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_B3 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416588887};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"B-3"};
        }
    }
}