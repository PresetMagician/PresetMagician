using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRhythmizerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297246241};

        protected override string PresetFile { get; } =
            "MMultiBandRhythmizerpresets.xml";

        protected override string RootTag { get; } = "MMultiBandRhythmizerpresetspresets";
    }
}