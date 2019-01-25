using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTransientMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296921714};

        protected override string PresetFile { get; } =
            "MMultiBandTransientpresets.xml";

        protected override string RootTag { get; } = "MMultiBandTransientpresetspresets";
    }
}