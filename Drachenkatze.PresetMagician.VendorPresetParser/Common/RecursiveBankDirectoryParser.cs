using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel.Logging;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public abstract class RecursiveBankDirectoryParser : AbstractVendorPresetParser
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

        public async Task<int> DoScanInternal(PresetBank rootBank, string directory, bool persist = true)
        {
            int count = 0;

            if (!Directory.Exists(directory))
            {
                PluginInstance.Plugin.Logger.Info($"Directory {directory} does not exist, skipping");
                return 0;
            }

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
                        var preset = new Preset
                        {
                            PresetName = file.Name.Replace("." + Extension, ""),
                            Plugin = PluginInstance.Plugin,
                            SourceFile = file.FullName,
                            PresetBank = rootBank
                        };

                        var data = ProcessFile(file.FullName, preset);

                        await PresetDataStorer.PersistPreset(preset, data);
                    }
                    catch (Exception e)
                    {
                        PluginInstance.Plugin.Logger.Error("Error processing preset {0} because of {1}", file.FullName,
                            e.Message);
                        PluginInstance.Plugin.Logger.Debug(e);
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