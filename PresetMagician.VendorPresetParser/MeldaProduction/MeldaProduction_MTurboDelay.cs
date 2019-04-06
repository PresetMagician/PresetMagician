using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboDelay : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297380676};

        protected override string PresetFile { get; } = "MTurboDelaypresets.xml";

        protected override string RootTag { get; } = "MTurboDelaypresets";
    }
}