using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public abstract class RecursiveBankDirectoryParser: AbstractVendorPresetParser
    {
        protected abstract string Extension { get; }

        public override void Init()
        {
            var directories = string.Join(",", (from dir in GetParseDirectories() select dir.directory));
            BankLoadingNotes = $"Presets are loaded from {directories}. First sub-folder defines the bank.";
        }

        protected virtual List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            return new List<(string, PresetBank)> {(GetParseDirectory(), GetRootBank())};
        }
        
        protected abstract string GetParseDirectory();

        protected virtual PresetBank GetRootBank()
        {
            return RootBank;
        }

        public override int GetNumPresets()
        {
            var count = 0;
            foreach (var parseDirectory in GetParseDirectories())
            {
                count += DoScanInternal(parseDirectory.presetBank, parseDirectory.directory, false).GetAwaiter()
                    .GetResult();
            }

            return count;
        }
        
        public override async Task DoScan()
        {
            foreach (var parseDirectory in GetParseDirectories())
            {
                await DoScanInternal(parseDirectory.presetBank, parseDirectory.directory);
            }
        }

        public async Task<int> DoScanInternal(PresetBank rootBank, string directory, bool persist=true)
        {
            int count = 0;
            var dirInfo = new DirectoryInfo(directory);
            var pattern = "*";

            if (Extension != "")
            {
                pattern = pattern + "." + Extension;
            }

            foreach (var file in dirInfo.EnumerateFiles(pattern))
            {
                count++;

                if (persist)
                {
                    try
                    {
                        var preset = new Preset {PresetName = file.Name.Replace("." + Extension, "")};
                        preset.Plugin = Plugin;
                        preset.SourceFile = file.FullName;
                        preset.PresetBank = rootBank;

                        var data = ProcessFile(file.FullName, preset);

                        await PresetDataStorer.PersistPreset(preset, data);
                    }
                    catch (Exception e)
                    {
                        Plugin.Error("Error processing preset {0} because of {1} {2}", file.FullName, e.Message, e);
                        Plugin.Debug(e.StackTrace);
                    }
                }
            }

            foreach (var subDirectory in dirInfo.EnumerateDirectories())
            {
                var bank = rootBank.CreateRecursive(subDirectory.Name);
             
                count += await DoScanInternal(bank, subDirectory.FullName, persist);
            }

            return count;
        }

        protected virtual byte[] ProcessFile(string fileName, Preset preset)
        {
            return File.ReadAllBytes(fileName);
        }
    }
}