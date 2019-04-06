using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MHarmonizerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299018344};

        protected override string PresetFile { get; } =
            "MMultiBandHarmonizerpresets.xml";

        protected override string RootTag { get; } = "MMultiBandHarmonizerpresetspresets";
    }
}