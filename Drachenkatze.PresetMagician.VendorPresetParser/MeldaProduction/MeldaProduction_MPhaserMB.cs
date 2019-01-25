using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPhaserMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {825308471};

        protected override string PresetFile { get; } =
            "MMultiBandPhaserpresets.xml";

        protected override string RootTag { get; } = "MMultiBandPhaserpresetspresets";
    }
}