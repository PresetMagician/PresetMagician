using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAGC : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298229091};

        protected override string PresetFile { get; } = "MAGCpresets.xml";
        protected override string RootTag { get; } = "MAGCpresets";
    }
}