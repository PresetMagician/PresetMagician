using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Syroot.Windows.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_miniBit: AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1833525876};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(new KnownFolder(KnownFolderType.PublicDocuments).Path,
                @"AudioThing\Presets\miniBit\Factory");
            
            DoScan(factoryBank, directory);
        }
    }
}
