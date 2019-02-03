using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_Repro5 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1968332597};

        protected override string GetProductName()
        {
            return "Repro-5";
        }
        protected override string GetDataDirectoryName()
        {
            return "Repro-1.data";
        }
    }
}