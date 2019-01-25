using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPowerSynth : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297380722};

        protected override string PresetFile { get; } = "MSynthesizerpresets.xml";

        protected override string RootTag { get; } = "Presets_MSynthesizerpresets";
    }
}