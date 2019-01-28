using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_Bazille : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1433421876};

        protected override string GetProductName()
        {
            return "Bazille";
        }
    }
}