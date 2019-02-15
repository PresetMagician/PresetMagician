using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public class Arturia_CS80 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1129535027};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"CS-80"};
        }
    }
}