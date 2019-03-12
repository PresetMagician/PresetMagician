using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_TripleCheese : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1667388281};

        protected override string GetProductName()
        {
            return "TripleCheese";
        }
    }
}