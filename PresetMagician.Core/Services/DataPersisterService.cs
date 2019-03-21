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
    public class DataPersisterService
    {
        private const string PluginStorageExtension = ".pmplugin";
        private const string PresetStorageExtension = ".pmpluginpresets";
        private const string TypesStorageFile = "Types.pmmc";
        private const string CharacteristicsStorageFile = "Characteristics.pmmc";

        public static string DefaultTypesCharacteristicsStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician");

        public static string DefaultPluginStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PluginData");

        private readonly GlobalService _globalService;
        private readonly VendorPresetParserService _vendorPresetParserService;
        private CerasSerializer _serializer;

        public DataPersisterService(GlobalService globalService, VendorPresetParserService vendorPresetParserService)
        {
            _globalService = globalService;
            _vendorPresetParserService = vendorPresetParserService;
        }

        private CerasSerializer GetSerializer()
        {
            if (_serializer == null)
            {
                var serializerConfig = new SerializerConfig();
                serializerConfig.DefaultTargets = TargetMember.None;
                serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
                _serializer = new CerasSerializer(serializerConfig);
            }

            return _serializer;
        }


        public void SaveTypesCharacteristics()
        {
            var serializer = GetSerializer();
            Directory.CreateDirectory(DefaultTypesCharacteristicsStoragePath);

            var typesDataFile = Path.Combine(DefaultTypesCharacteristicsStoragePath, TypesStorageFile);
            var typesData = serializer.Serialize(Type.GlobalTypes);

            File.WriteAllBytes(typesDataFile, typesData);

            var characteristicsDataFile =
                Path.Combine(DefaultTypesCharacteristicsStoragePath, CharacteristicsStorageFile);
            var characteristicsData = serializer.Serialize(Characteristic.GlobalCharacteristics);

            File.WriteAllBytes(characteristicsDataFile, characteristicsData);
        }

        public void LoadTypesCharacteristics(CerasSerializer serializer)
        {
            var typesDataFile = Path.Combine(DefaultTypesCharacteristicsStoragePath, TypesStorageFile);

            if (File.Exists(typesDataFile))
            {
                var types = serializer.Deserialize<EditableCollection<Type>>(File.ReadAllBytes(typesDataFile));
                Type.GlobalTypes.Clear();
                Type.GlobalTypes.AddRange(types);
            }

            var characteristicsDataFile =
                Path.Combine(DefaultTypesCharacteristicsStoragePath, CharacteristicsStorageFile);

            if (File.Exists(characteristicsDataFile))
            {
                var characteristics =
                    serializer.Deserialize<EditableCollection<Characteristic>>(
                        File.ReadAllBytes(characteristicsDataFile));
                Characteristic.GlobalCharacteristics.Clear();
                Characteristic.GlobalCharacteristics.AddRange(characteristics);
            }
        }

        public void SavePlugin(Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = GetPluginStorageFile(plugin);

            var data = GetSerializer().Serialize(plugin);

            File.WriteAllBytes(dataFile, data);

            SaveTypesCharacteristics();
        }

        public string GetPluginStorageFile(Plugin plugin)
        {
            return Path.Combine(DefaultPluginStoragePath, plugin.PluginId + PluginStorageExtension);
        }

        public string GetPresetsStorageFile(Plugin plugin)
        {
            return Path.Combine(DefaultPluginStoragePath, plugin.PluginId + PresetStorageExtension);
        }

        public Plugin LoadPlugin(string fileName)
        {
            return LoadPlugin(GetSerializer(), fileName);
        }

        public Plugin LoadPlugin(CerasSerializer serializer, string fileName)
        {
            var dataFile = Path.Combine(DefaultPluginStoragePath, fileName);

            var plugin = serializer.Deserialize<Plugin>(File.ReadAllBytes(dataFile));

            foreach (var pluginLocation in plugin.PluginLocations)
            {
                if (pluginLocation.GetSavedPresetParserClassName() != null)
                {
                    pluginLocation.PresetParser =
                        _vendorPresetParserService.GetVendorPresetParserByName(pluginLocation
                            .GetSavedPresetParserClassName());
                }
            }

            return plugin;
        }

        public List<string> GetStoredPluginFiles()
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var list = new List<string>();

            foreach (var file in Directory.EnumerateFiles(
                DefaultPluginStoragePath, "*" + PluginStorageExtension, SearchOption.AllDirectories))
            {
                list.Add(file);
            }

            return list;
        }

        public void Load()
        {
            LoadPlugins();
        }

        public void Save()
        {
            SavePlugins();
        }

        private void LoadPlugins()
        {
            var serializer = GetSerializer();
            LoadTypesCharacteristics(serializer);
            _globalService.Plugins.Clear();

            var pluginFiles = GetStoredPluginFiles();

            foreach (var filename in pluginFiles)
            {
                _globalService.Plugins.Add(LoadPlugin(serializer, filename));
            }
        }

        public void SavePlugins()
        {
            var savedPluginFiles = new HashSet<string>();
            foreach (var plugin in _globalService.Plugins)
            {
                SavePlugin(plugin);
                savedPluginFiles.Add(GetPluginStorageFile(plugin));
            }

            var pluginFiles = GetStoredPluginFiles();
            foreach (var removedPlugin in pluginFiles.Except(savedPluginFiles))
            {
                File.Delete(removedPlugin);
            }
        }

        public void SavePresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            var data = GetSerializer().Serialize(plugin.Presets);

            File.WriteAllBytes(presetsStorageFile, data);

            SaveTypesCharacteristics();
        }

        public void LoadPresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            if (File.Exists(presetsStorageFile))
            {
                var presets = GetSerializer()
                    .Deserialize<EditableCollection<Preset>>(File.ReadAllBytes(presetsStorageFile));
                plugin.Presets = presets;
            }
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

            if (File.Exists(PresetDataPersisterService.DefaultDatabasePath))
            {
                fileInfo = new FileInfo(PresetDataPersisterService.DefaultDatabasePath);
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
                stat.PresetUncompressedSize = (from p in plugin.Presets select p.PresetSize).Sum();
                stat.PresetCompressedSize = (from p in plugin.Presets select p.PresetCompressedSize).Sum();
                stats.Add(stat);
            }

            return stats;
        }
    }
}