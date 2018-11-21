using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group.PunchBox
{
    public class D16PunchBox : IVendorPresetParser
    {
        public List<int> SupportedPlugins => new List<int> { 1347306072 };
    }
}