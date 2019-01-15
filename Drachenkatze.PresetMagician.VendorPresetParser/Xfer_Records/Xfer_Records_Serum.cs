using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.Utils;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Xfer_Records
{
    public class Xfer_Records_Serum: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1483109208 };

        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Xfer\Serum Presets\Presets");
            
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);
            
            DoScan(factoryBank, directory);
        }
        
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            foreach (var file in dirInfo.EnumerateFiles("*.fxp"))
            {
                var fxp = new FXP();
                fxp.ReadFile(file.FullName);

                var preset = new Preset {PresetName = fxp.Name.Trim('\0'), PresetBank = rootBank};
                preset.SetPlugin(Plugin);

                preset.PresetData = fxp.ChunkDataByteArray;
                Presets.Add(preset);
            }

            foreach (var subDirectory in dirInfo.EnumerateDirectories())
            {
                var bank = new PresetBank
                {
                    BankName = subDirectory.Name
                };

                DoScan(bank, subDirectory.FullName);
                rootBank.PresetBanks.Add(bank);
            }
        }
    }
}