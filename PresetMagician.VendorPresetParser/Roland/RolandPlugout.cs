using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Roland.Internal;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    public abstract class RolandPlugoutParser : AbstractVendorPresetParser
    {
        private RolandExportConfig ExportConfig;
        private List<string> PresetFiles = new List<string>();

        public override void Init()
        {
            BankLoadingNotes = $"Presets will be loaded from {GetPresetFolder()}";
            base.Init();
        }

        public string GetPresetFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Roland Cloud", GetProductName());
        }

        public override int GetNumPresets()
        {
            InitializeInternal();

            var numPresets = PresetFiles.Count * ExportConfig.KoaNumPresets;
            return base.GetNumPresets() + numPresets;
        }

        public override async Task DoScan()
        {
            InitializeInternal();
            var parser = new KoaBankFileParser(ExportConfig.KoaFileHeader, ExportConfig.KoaPresetNameLength,
                ExportConfig.KoaPresetLength,
                ExportConfig.KoaNumPresets);

            var converter = new RolandConverter(ExportConfig);
            converter.LoadDefinitionFromCsvString(Encoding.UTF8.GetString(GetDefinitionData()));

            foreach (var presetFile in PresetFiles)
            {
                var presets = parser.Parse(presetFile);
                var presetBankName = Path.GetFileNameWithoutExtension(presetFile);

                foreach (var parsedPreset in presets)
                {
                    converter.SetFileMemory(parsedPreset.PresetData);

                    var preset = new PresetParserMetadata
                    {
                        PresetName = parsedPreset.PresetName.Trim(), Plugin = PluginInstance.Plugin,
                        BankPath = presetBankName,
                        SourceFile = presetFile + ":" + parsedPreset.Index
                    };

                    PostProcessPreset(preset, converter);

                    await DataPersistence.PersistPreset(preset, converter.Export());
                }
            }

            await base.DoScan();
        }

        protected virtual void PostProcessPreset(PresetParserMetadata metadata, RolandConverter converter)
        {
        }

        private void InitializeInternal()
        {
            ExportConfig =
                JsonConvert.DeserializeObject<RolandExportConfig>(Encoding.UTF8.GetString(GetExportConfig()));

            ExportConfig.Suffix = GetSuffixData();

            PresetFiles.Clear();
            foreach (var patchFile in Directory.EnumerateFiles(
                GetPresetFolder(), "*.bin",
                SearchOption.AllDirectories))
            {
                PresetFiles.Add(patchFile);
            }
        }

        protected abstract string GetProductName();

        protected abstract byte[] GetExportConfig();

        public abstract byte[] GetSuffixData();

        public abstract byte[] GetDefinitionData();
    }
}