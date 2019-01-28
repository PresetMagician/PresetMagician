using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_Zebra2 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397572658};

        protected override string GetProductName()
        {
            return "Zebra2";
        }
    }
}