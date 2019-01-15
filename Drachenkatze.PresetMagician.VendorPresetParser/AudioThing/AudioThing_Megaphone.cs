using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{    
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_Megaphone : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298485352};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\Megaphone");

            DoScan(factoryBank, directory);
        }
    }
}