using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    class AudioThing_miniBitCM: AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1833525827};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing Presets\miniBitCM");

            DoScan(factoryBank, directory);
        }
    }
}
