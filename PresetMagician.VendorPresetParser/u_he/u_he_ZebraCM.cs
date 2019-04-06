using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_ZebraCM : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1516593997};

        protected override string GetProductName()
        {
            return "ZebraCM";
        }
    }
}