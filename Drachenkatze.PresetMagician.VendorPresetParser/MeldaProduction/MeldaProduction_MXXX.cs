using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MXXX : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297635377, 1297635416};

        protected override string PresetFile { get; } = "MXXXpresets.xml";

        protected override string RootTag { get; } = "Presets_MMXXXCreativepresets";
    }
}