using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MEqualizerLP : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131434};

        protected override string PresetFile { get; } = "MEqualizerLinearPhasepresets.xml";

        protected override string RootTag { get; } = "MEqualizerpresets";
    }
}