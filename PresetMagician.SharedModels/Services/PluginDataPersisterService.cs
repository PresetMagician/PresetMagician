using System;
using System.Collections.Generic;
using System.IO;
using Ceras;
using SharedModels.NewModels;
using Path = Catel.IO.Path;

namespace SharedModels.Services
{
    public class PluginDataPersisterService
    {
        private const string PluginStorageExtension = ".pmplugin";
        public static string DefaultPluginStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PluginData");
        
        private readonly CerasSerializer _serializer;

        public PluginDataPersisterService()
        {
            var serializerConfig = new SerializerConfig();
            serializerConfig.DefaultTargets = TargetMember.None;
            serializerConfig.KnownTypes.Add(typeof(Plugin));
            serializerConfig.KnownTypes.Add(typeof(Characteristic));
            serializerConfig.KnownTypes.Add(typeof(Preset));
            serializerConfig.KnownTypes.Add(typeof(PluginLocation));
            serializerConfig.KnownTypes.Add(typeof(Type));

            serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
            _serializer = new CerasSerializer(serializerConfig);
        }

        public void SavePlugin(Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = Path.Combine(DefaultPluginStoragePath, plugin.PluginId + PluginStorageExtension);

            var data = _serializer.Serialize(plugin);

            File.WriteAllBytes(dataFile, data);
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
    }
}