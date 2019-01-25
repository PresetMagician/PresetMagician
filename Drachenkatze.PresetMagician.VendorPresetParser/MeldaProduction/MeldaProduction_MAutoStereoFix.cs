using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoStereoFix : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298232166};

        protected override string PresetFile { get; } = "MAutoStereoFixpresets.xml";

        protected override string RootTag { get; } = "MAutoStereoFixpresets";
    }
}