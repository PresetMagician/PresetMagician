using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFreeformEqualizer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131409};

        protected override string PresetFile { get; } =
            "MFreeformEqualizerpresets.xml";

        protected override string RootTag { get; } = "MFreeformEqualizerpresetspresets";
    }
}