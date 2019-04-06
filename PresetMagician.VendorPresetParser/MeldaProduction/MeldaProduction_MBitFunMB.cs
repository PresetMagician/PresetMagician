using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MBitFunMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298285126};

        protected override string PresetFile { get; } = "MMultiBandBitFunpresets.xml";

        protected override string RootTag { get; } = "MMultiBandBitFunpresets";
    }
}