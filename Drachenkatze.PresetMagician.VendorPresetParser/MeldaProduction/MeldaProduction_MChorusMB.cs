using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MChorusMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296917352};

        protected override string PresetFile { get; } =
            "MMultiBandChoruspresets.xml";

        protected override string RootTag { get; } = "MMultiBandChoruspresetspresets";
    }
}