using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_MFM2 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296452914};

        protected override string GetProductName()
        {
            return "MFM2";
        }
    }
}