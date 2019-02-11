using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_UhbikD : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1432568881};

        protected override string GetProductName()
        {
            return "Uhbik-D";
        }
        
        protected override string GetDataDirectoryName()
        {
            return "Uhbik.data";
        }
    }
}