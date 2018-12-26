using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public class AudioThing_HandClapper : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1212363341};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\HandClapper");

            DoScan(factoryBank, directory);
        }
    }
}