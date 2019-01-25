using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MMultiAnalyzer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296908654};

        protected override string PresetFile { get; } = "MMultiAnalyzerpresets.xml";

        protected override string RootTag { get; } = "MMultiAnalyzerpresets";
    }
}