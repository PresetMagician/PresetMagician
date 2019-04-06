using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSpectralDynamics : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296114745, 1296114801};

        protected override string PresetFile { get; } =
            "MSpectralDynamicspresets.xml";

        protected override string RootTag { get; } = "MSpectralDynamicspresetspresets";
    }
}