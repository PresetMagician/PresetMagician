using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MUltraMaximizer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296133496};

        protected override string PresetFile { get; } = "MUltraMaximizerpresets.xml";

        protected override string RootTag { get; } = "MUltraMaximizerpresetspresets";
    }
}