using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MCombMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296909167};

        protected override string PresetFile { get; } = "MMultiBandCombpresets.xml";

        protected override string RootTag { get; } = "MMultiBandCombpresets";
    }
}