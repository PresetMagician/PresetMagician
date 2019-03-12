using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Collections;
using Ceras;
using PresetMagician.Core.Models;
using Path = Catel.IO.Path;

namespace PresetMagician.Core.Services
{
    public class DataPersisterService
    {
        private const string PluginStorageExtension = ".pmplugin";
        private const string PluginLocationsStorageExtension = ".pmpluginlocations";
        public static string DefaultPluginStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PluginData");
        
        private readonly CerasSerializer _serializer;

        public DataPersisterService()
        {
            var serializerConfig = new SerializerConfig();
            serializerConfig.DefaultTargets = TargetMember.None;

            //serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
            _serializer = new CerasSerializer(serializerConfig);
        }

        public void SavePlugin(Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = GetPluginStorageFile(plugin);

            var data = _serializer.Serialize(plugin);

            File.WriteAllBytes(dataFile, data);
        }

        public string GetPluginStorageFile(Plugin plugin)
        {
            return Path.Combine(DefaultPluginStoragePath, plugin.PluginId + PluginStorageExtension);
        }

        public Plugin LoadPlugin(string fileName)
        {
            var dataFile = Path.Combine(DefaultPluginStoragePath, fileName);

            return _serializer.Deserialize<Plugin>(File.ReadAllBytes(dataFile));
        }

        public List<string> GetStoredPluginFiles()
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);
            
            var list = new List<string>();

            foreach (var file in Directory.EnumerateFiles(
                DefaultPluginStoragePath, "*"+PluginStorageExtension, SearchOption.AllDirectories))
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
            Plugins.Clear();

            var pluginFiles = GetStoredPluginFiles();

            foreach (var filename in pluginFiles)
            {
                Plugins.Add(LoadPlugin(filename));
            }
        }

        private void SavePlugins()
        {
            var savedPluginFiles = new HashSet<string>();
            foreach (var plugin in Plugins)
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
        
        public FastObservableCollection<Plugin> Plugins { get; } = new FastObservableCollection<Plugin>();
    }
}