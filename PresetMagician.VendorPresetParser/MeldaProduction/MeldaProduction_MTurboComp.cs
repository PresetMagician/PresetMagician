using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboComp : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297380675};

        protected override string PresetFile { get; } = "MTurboComppresets.xml";

        protected override string RootTag { get; } = "MTurboComppresets";
    }
}