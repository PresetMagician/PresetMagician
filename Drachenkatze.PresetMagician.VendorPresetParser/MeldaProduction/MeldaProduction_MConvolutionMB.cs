using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MConvolutionMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296909135};

        protected override string PresetFile { get; } =
            "MMultiBandConvolutionpresets.xml";

        protected override string RootTag { get; } = "MMultiBandConvolutionpresetspresets";
    }
}