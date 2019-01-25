using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoEqualizer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131454};

        protected override string PresetFile { get; } = "MAutoEqualizerpresets.xml";

        protected override string RootTag { get; } = "MEqualizerpresets";
    }
}