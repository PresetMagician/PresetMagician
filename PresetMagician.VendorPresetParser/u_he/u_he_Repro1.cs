using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_Repro1 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1968332593};

        protected override string GetProductName()
        {
            return "Repro-1";
        }
    }
}