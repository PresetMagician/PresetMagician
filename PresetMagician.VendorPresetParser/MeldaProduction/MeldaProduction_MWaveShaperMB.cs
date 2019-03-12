using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWaveShaperMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011443};

        protected override string PresetFile { get; } =
            "MMultiBandWaveShaperpresets.xml";

        protected override string RootTag { get; } = "MMultiBandWaveShaperpresetspresets";
    }
}