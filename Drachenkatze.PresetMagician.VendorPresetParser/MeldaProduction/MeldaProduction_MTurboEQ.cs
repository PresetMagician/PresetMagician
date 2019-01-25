using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTurboEQ : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297380677};

        protected override string PresetFile { get; } = "MTurboEQpresets.xml";

        protected override string RootTag { get; } = "MTurboEQpresets";
    }
}