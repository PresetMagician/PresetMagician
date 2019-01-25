using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_V76Pre : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1413827653};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"V76-Pre"};
        }
    }
}