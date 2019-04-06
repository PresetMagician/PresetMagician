using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDistortionMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298426227};

        protected override string PresetFile { get; } =
            "MMultiBandDistortionpresets.xml";

        protected override string RootTag { get; } = "MMultiBandDistortionpresetspresets";
    }
}