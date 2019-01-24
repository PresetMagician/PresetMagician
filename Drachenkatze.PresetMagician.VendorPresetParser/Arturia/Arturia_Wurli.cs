using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Wurli : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1465209394};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Wurli"};
        }
    }
}