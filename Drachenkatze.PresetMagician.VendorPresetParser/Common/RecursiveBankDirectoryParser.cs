using System;
using System.Collections.ObjectModel;
using System.IO;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class RecursiveBankDirectoryParser
    {
        protected string Extension;
        protected IVstPlugin _vstPlugin;
        protected ObservableCollection<Preset> Presets { get; }
        
        public RecursiveBankDirectoryParser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets)
        {
            _vstPlugin = vstPlugin;
            Extension = extension;
            Presets = presets;
        }
        
        public void DoScan(PresetBank rootBank, string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            var pattern = "*";

            if (Extension != "")
            {
                pattern = pattern + "." + Extension;
            }
            
            foreach (var file in dirInfo.EnumerateFiles(pattern))
            {
                try
                {
                    Preset preset = new Preset();
                    preset.PresetName = file.Name.Replace("." + Extension, "");
                    preset.SetPlugin(_vstPlugin);
                    preset.PresetBank = rootBank;

                    ProcessFile(file.FullName, preset);
                    
                    Presets.Add(preset);
                } catch (Exception e)
                {
                    LogTo.Error("Error processing preset {0} because of {1} {2}", file.FullName, e.Message, e);
                    LogTo.Debug(e.StackTrace);
                }
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

        protected virtual void ProcessFile(string fileName, Preset preset)
        {
            preset.PresetData = File.ReadAllBytes(fileName);
        }
    }
}