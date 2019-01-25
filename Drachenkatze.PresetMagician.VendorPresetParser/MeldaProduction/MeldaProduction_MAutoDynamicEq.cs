using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoDynamicEq : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298430273};

        protected override string PresetFile { get; } = "MAutoDynamicEqpresets.xml";

        protected override string RootTag { get; } = "presets";
    }
}