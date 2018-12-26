using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.Utils;
using GSF;

namespace Drachenkatze.PresetMagician.VendorPresetParser.HumanoidSoundSystems
{
    public class HumanoidSoundSystems_Enzyme: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1162762841 };

        private string rootDirectory; 
        public void ScanBanks()
        {
            rootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Humanoid Sound Systems\Enzyme\EnzymeData\Presets");
            
           
            DoScan(RootBank, rootDirectory);
        }
        
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            foreach (var file in dirInfo.EnumerateFiles("*.enz"))
            {
                var data = File.ReadAllBytes(file.FullName);
                var ms = new MemoryStream();
                ms.Write(data,0, data.Length);
                
                var preset = new Preset
                {
                    PresetName = file.Name.Replace(".enz", ""), PresetBank = rootBank
                };
                preset.SetPlugin(VstPlugin);

                var relativeFile = BinaryFile.StringToByteArray(file.FullName.Replace(rootDirectory, ""));
                var tuningFile = BinaryFile.StringToByteArray(@"Tunings\Basic\Default.tun");
                
                ms.Seek(0, SeekOrigin.End);
                ms.WriteByte(0x01);
                ms.WriteByte((byte)relativeFile.Length);
                ms.Write(relativeFile, 0, relativeFile.Length);
                
                ms.WriteByte(0x01);
                ms.WriteByte((byte)tuningFile.Length);
                ms.Write(tuningFile, 0, tuningFile.Length);

                ms.Seek(0, SeekOrigin.Begin);

                var ms2 = new MemoryStream();
                ms2.Write(LittleEndian.GetBytes(ms.Length),0,4);
                ms.WriteTo(ms2);
                ms2.Seek(0, SeekOrigin.Begin);

                preset.PresetData = ms2.ToArray();
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