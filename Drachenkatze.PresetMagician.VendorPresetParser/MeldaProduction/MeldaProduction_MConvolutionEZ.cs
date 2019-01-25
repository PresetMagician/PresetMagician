using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MConvolutionEZ : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296262522};

        protected override string PresetFile { get; } = "MConvolutionEZpresets.xml";

        protected override string RootTag { get; } = "MConvolutionEZpresets";
    }
}