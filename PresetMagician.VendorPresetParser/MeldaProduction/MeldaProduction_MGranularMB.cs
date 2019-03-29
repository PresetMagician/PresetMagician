using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MGranularMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296910194};

        protected override string PresetFile { get; } =
            "MMultiBandGranularpresets.xml";

        protected override string RootTag { get; } = "MMultiBandGranularpresetspresets";
    }
}