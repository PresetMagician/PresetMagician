using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    internal interface IVendorPresetParser
    {
        List<int> SupportedPlugins { get; }
    }
}