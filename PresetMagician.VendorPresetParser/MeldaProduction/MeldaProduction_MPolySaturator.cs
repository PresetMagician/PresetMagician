using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MPolySaturator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297117011};

        protected override string PresetFile { get; } = "MPolySaturatorpresets.xml";

        protected override string RootTag { get; } = "MPolySaturatorpresets";
    }
}