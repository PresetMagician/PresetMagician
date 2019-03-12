using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDynamicsMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131368, 1296131424};

        protected override string PresetFile { get; } =
            "MMultiBandDynamicspresets.xml";

        protected override string RootTag { get; } = "MMultiBandDynamicspresetspresets";
    }
}