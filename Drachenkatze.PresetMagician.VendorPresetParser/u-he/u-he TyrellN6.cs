using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    class u_he_TyrellN6 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1952017974 };

        public void ScanBanks()
        {
            Banks = new List<PresetBank>();

            Debug.WriteLine(ResolveShortcut(getDataDirectory("TyrellN6.data.lnk")));

            
        }

        
    }
}
