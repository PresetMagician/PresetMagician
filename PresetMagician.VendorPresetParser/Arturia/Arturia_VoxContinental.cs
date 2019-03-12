using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_VoxContinental : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1450145842};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"VOX Continental"};
        }
    }
}