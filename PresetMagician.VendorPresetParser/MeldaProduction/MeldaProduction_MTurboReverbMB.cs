using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboReverbMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299468909};

        protected override string PresetFile { get; } = "MTurboReverbMBpresets.xml";

        protected override string RootTag { get; } = "MTurboReverbMBpresets";
    }
}