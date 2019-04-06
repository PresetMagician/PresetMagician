using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_UhbikG : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432569649};

        protected override string GetProductName()
        {
            return "Uhbik-G";
        }

        protected override string GetDataDirectoryName()
        {
            return "Uhbik.data";
        }
    }
}