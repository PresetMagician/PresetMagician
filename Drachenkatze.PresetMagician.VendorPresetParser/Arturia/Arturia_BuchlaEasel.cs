using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_BuchlaEasel : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1114981729};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Buchla Easel"};
        }
    }
}