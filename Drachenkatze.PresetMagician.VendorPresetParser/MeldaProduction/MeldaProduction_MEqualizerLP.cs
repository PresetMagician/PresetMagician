using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
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