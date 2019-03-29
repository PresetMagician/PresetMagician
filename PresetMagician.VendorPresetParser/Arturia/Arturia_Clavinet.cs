using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_Clavinet : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1131176310};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Clavinet"};
        }
    }
}