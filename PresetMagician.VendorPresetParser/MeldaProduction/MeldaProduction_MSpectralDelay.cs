using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSpectralDelay : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297313892};

        protected override string PresetFile { get; } = "MSpectralDelaypresets.xml";

        protected override string RootTag { get; } = "MSpectralDelaypresets";
    }
}