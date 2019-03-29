using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_BazilleCM : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1967276877};

        protected override string GetProductName()
        {
            return "BazilleCM";
        }
    }
}