using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Collections;
using Ceras;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using Path = Catel.IO.Path;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        private const string PluginStorageExtension = ".pmplugin";
        private const string PresetStorageExtension = ".pmpluginpresets";
        private const string TypesStorageFile = "Types.pmmc";
        
        private const string CharacteristicsStorageFile = "Characteristics.pmmc";
        private const string PreviewNotePlayersStorageFile = "PreviewNotePlayers.pmmc";
        private const string DontShowAgainDialogsStorageFile = "DontShowAgainDialogs.pmmc";
        private const string RememberMyChoiceResults = "RememberMyChoiceResults.pmmc";

        public static string DefaultDataStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\Data");

        public static string DefaultPluginStoragePath = "PluginData";

        private readonly GlobalService _globalService;
        private readonly VendorPresetParserService _vendorPresetParserService;
        private CerasSerializer _loadSerializer;
        private CerasSerializer _saveSerializer;

        public DataPersisterService(GlobalService globalService, VendorPresetParserService vendorPresetParserService)
        {
            _globalService = globalService;
            _vendorPresetParserService = vendorPresetParserService;
        }

        private CerasSerializer GetLoadSerializer()
        {
            if (_loadSerializer == null)
            {
                var serializerConfig = new SerializerConfig {DefaultTargets = TargetMember.None};
                serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
                _loadSerializer = new CerasSerializer(serializerConfig);
            }

            return _loadSerializer;
        }
        
        private CerasSerializer GetSaveSerializer()
        {
            if (_saveSerializer == null)
            {
                var serializerConfig = new SerializerConfig {DefaultTargets = TargetMember.None};
                serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
                _saveSerializer = new CerasSerializer(serializerConfig);
            }

            return _saveSerializer;
        }


        public void Load()
        {
            LoadPlugins();
        }

        public void Save()
        {
            SavePlugins();
        }

        public long GetTotalDataSize()
        {
            long totalCount = 0;

            var files = GetStoredPluginFiles();
            FileInfo fileInfo;

            foreach (var file in files)
            {
                fileInfo = new FileInfo(file);
                totalCount += fileInfo.Length;
            }

            if (File.Exists(PresetDataPersisterService.GetDatabaseFile()))
            {
                fileInfo = new FileInfo(PresetDataPersisterService.GetDatabaseFile());
                totalCount += fileInfo.Length;
            }

            return totalCount;
        }

        public List<PresetDatabaseStatistic> GetStorageStatistics()
        {
            var stats = new List<PresetDatabaseStatistic>();
            foreach (var plugin in _globalService.Plugins)
            {
                var stat = new PresetDatabaseStatistic();
                stat.PluginName = plugin.PluginName;
                stat.PresetCount = plugin.Presets.Count;
                stat.PresetUncompressedSize = (from p in plugin.Presets select (long)p.PresetSize).Sum();
                stat.PresetCompressedSize = (from p in plugin.Presets select (long)p.PresetCompressedSize).Sum();
                stats.Add(stat);
            }

            return stats;
        }
    }
}