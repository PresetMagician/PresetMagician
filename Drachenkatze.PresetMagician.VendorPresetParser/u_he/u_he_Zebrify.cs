using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class u_he_Zebrify : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397578034};

        protected override string GetProductName()
        {
            return "Zebrify";
        }
        
        protected override string GetDataDirectoryName()
        {
            return "Zebra2.data";
        }
    }
}