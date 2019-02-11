using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    internal class u_he_TyrellN6 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1952017974};

        protected override string GetProductName()
        {
            return "TyrellN6";
        }
    }
}