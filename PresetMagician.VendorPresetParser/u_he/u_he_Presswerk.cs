using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_Presswerk : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1969770583};

        protected override string GetProductName()
        {
            return "Presswerk";
        }
    }
}