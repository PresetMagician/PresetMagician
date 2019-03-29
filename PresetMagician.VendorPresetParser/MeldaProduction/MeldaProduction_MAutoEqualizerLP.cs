using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoEqualizerLP : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296137778};

        protected override string PresetFile { get; } = "MAutoEqualizerLinearPhasepresets.xml";

        protected override string RootTag { get; } = "presets";
    }
}