using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class RecursiveBankDirectoryParser
    {
        protected readonly string Extension;
        protected readonly Plugin _plugin;
        protected readonly IPresetDataStorer _presetDataStorer;

        public RecursiveBankDirectoryParser(Plugin plugin, string extension, IPresetDataStorer presetDataStorer)
        {
            _plugin = plugin;
            Extension = extension;
            _presetDataStorer = presetDataStorer;
        }

        public async Task DoScan(PresetBank rootBank, string directory)
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
                    var preset = new Preset {PresetName = file.Name.Replace("." + Extension, "")};
                    preset.Plugin = _plugin;
                    preset.SourceFile = file.FullName;
                    preset.PresetBank = rootBank;

                    var data = ProcessFile(file.FullName, preset);

                    await _presetDataStorer.PersistPreset(preset, data);
                }
                catch (Exception e)
                {
                    _plugin.Error("Error processing preset {0} because of {1} {2}", file.FullName, e.Message, e);
                    _plugin.Debug(e.StackTrace);
                }
            }

            foreach (var subDirectory in dirInfo.EnumerateDirectories())
            {
                var bank = rootBank.CreateRecursive(subDirectory.Name);
             
                await DoScan(bank, subDirectory.FullName);
            }
        }

        protected virtual byte[] ProcessFile(string fileName, Preset preset)
        {
            return File.ReadAllBytes(fileName);
        }
    }
}