using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_MiniV3 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296649779};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Mini"};
        }
    }
}