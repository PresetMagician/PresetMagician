using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.RevealSound.Internal;

namespace PresetMagician.VendorPresetParser.RevealSound
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class RevealSound_Spire: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1399878194};
        
        public override void Init()
        {
            BankLoadingNotes = $"Factory Presets will be loaded from {GetFactoryPresetFolder()}. User Presets will "+
                $"be loaded from {GetUserPresetFolder()}.";
            base.Init();
        }

        public override int GetNumPresets()
        {

            var numPresets = 0;

            foreach (var bankFile in GetBankFiles())
            {
                Logger.Debug($"Parsing {bankFile}");

                try
                {
                    var bf = new SpireBank();
                    bf.ParseDiskFile(File.ReadAllBytes(bankFile));
                    numPresets += bf.Presets.Count;
                }
                catch (SpireException e)
                {
                    Logger.Error($"Error parsing {bankFile}");
                    Logger.LogException(e);
                }
            }
            
            return base.GetNumPresets() + numPresets;
        }
        
        public override async Task DoScan()
        {
            
            foreach (var bankFile in GetBankFiles())
            {
                Logger.Debug($"Parsing {bankFile}");

                try
                {
                    var cfg = new SpireJsonConfig {SelectedBank = bankFile};

                    var bf = new SpireBank();
                    bf.ParseDiskFile(File.ReadAllBytes(bankFile));

                    foreach (var p in bf.Presets)
                    {
                        var bankPath = Path.GetFileNameWithoutExtension(bankFile);
                        
                        var preset = new PresetParserMetadata
                        {
                            PresetName = p.ProgramName.Trim(), Plugin = PluginInstance.Plugin,
                            BankPath = bankPath,
                            SourceFile = bankFile+":"+ bf.Presets.IndexOf(p)
                        };


                        
                        await DataPersistence.PersistPreset(preset, bf.GenerateMemoryBank(p, cfg));
                    }
                }
                catch (SpireException e)
                {
                    Logger.Error($"Error parsing {bankFile}");
                    Logger.LogException(e);
                }
            }
           
            await base.DoScan();
        }
        
        private string GetFactoryPresetFolder()
        {
            return Path.GetDirectoryName(PluginInstance.Plugin.DllPath);
        }

        private string GetUserPresetFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"RevealSound\Banks");
        }

        private List<string> GetBankFiles(string path)
        {
            return Directory.EnumerateFiles(path, "*.sbf", SearchOption.AllDirectories).ToList();
        }

        private List<string> GetBankFiles()
        {
            return GetBankFiles(GetFactoryPresetFolder()).Concat(GetBankFiles(GetUserPresetFolder())).ToList();
        }
    }
}