using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_Zebralette : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397572659};

        protected override string GetProductName()
        {
            return "Zebralette";
        }
        
        protected override string GetDataDirectoryName()
        {
            return "Zebra2.data";
        }
    }
}