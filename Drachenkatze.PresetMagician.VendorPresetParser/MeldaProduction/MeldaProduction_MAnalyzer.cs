using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAnalyzer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131426};

        protected override string PresetFile { get; } = "MAnalyzerpresets.xml";

        protected override string RootTag { get; } = "MAnalyzerpresetspresets";
    }
}