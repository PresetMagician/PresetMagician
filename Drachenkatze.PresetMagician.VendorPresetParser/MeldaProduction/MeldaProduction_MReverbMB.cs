using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MReverbMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {875848759};

        protected override string PresetFile { get; } =
            "MMultiBandReverbpresets.xml";

        protected override string RootTag { get; } = "MMultiBandReverbpresetspresets";
    }
}