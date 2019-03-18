using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Catel.Collections;
using Catel.Reflection;
using Ceras;
using Ceras.Resolvers;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using Path = Catel.IO.Path;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.Core.Services
{
    public class DataPersisterService
    {
        private const string PluginStorageExtension = ".pmplugin";
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

        public DataPersisterService(GlobalService globalService, VendorPresetParserService vendorPresetParserService)
        {
            _globalService = globalService;
            _vendorPresetParserService = vendorPresetParserService;
        }

        private CerasSerializer GetSerializer()
        {
            var serializerConfig = new SerializerConfig();
            serializerConfig.DefaultTargets = TargetMember.None;
            serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
            return new CerasSerializer(serializerConfig);
        }


        public void SavePlugin(Plugin plugin)
        {
            SavePlugin(GetSerializer(), plugin);
        }

        public void SaveTypesCharacteristics(CerasSerializer serializer)
        {
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

        public void SavePlugin(CerasSerializer serializer, Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = GetPluginStorageFile(plugin);

            var data = serializer.Serialize(plugin);

            File.WriteAllBytes(dataFile, data);
        }

        public string GetPluginStorageFile(Plugin plugin)
        {
            return Path.Combine(DefaultPluginStoragePath, plugin.PluginId + PluginStorageExtension);
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
                        _vendorPresetParserService.GetVendorPresetParserByName(pluginLocation.GetSavedPresetParserClassName());
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

        private void SavePlugins()
        {
            var serializer = GetSerializer();

            var savedPluginFiles = new HashSet<string>();
            foreach (var plugin in _globalService.Plugins)
            {
                SavePlugin(serializer, plugin);
                savedPluginFiles.Add(GetPluginStorageFile(plugin));
            }

            var pluginFiles = GetStoredPluginFiles();
            foreach (var removedPlugin in pluginFiles.Except(savedPluginFiles))
            {
                File.Delete(removedPlugin);
            }

            SaveTypesCharacteristics(serializer);
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