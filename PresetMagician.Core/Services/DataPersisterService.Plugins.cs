using System.Collections.Generic;
using System.IO;
using System.Linq;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        public string GetPluginsStoragePath()
        {
            var pluginsStoragePath = Path.Combine(DefaultDataStoragePath, DefaultPluginStoragePath);
            Directory.CreateDirectory(pluginsStoragePath);
            return pluginsStoragePath;
        }

        public string GetPluginStorageFile(Plugin plugin)
        {
            return Path.Combine(GetPluginsStoragePath(), plugin.PluginId + PluginStorageExtension);
        }

        public List<string> GetStoredPluginFiles()
        {
            var list = new List<string>();

            foreach (var file in Directory.EnumerateFiles(
                GetPluginsStoragePath(), "*" + PluginStorageExtension, SearchOption.AllDirectories))
            {
                list.Add(file);
            }

            return list;
        }

        public void SavePlugin(Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = GetPluginStorageFile(plugin);

            var data = GetSaveSerializer().Serialize(plugin);

            File.WriteAllBytes(dataFile, data);

            SaveTypesCharacteristics();
            SavePreviewNotePlayers();
        }


        public Plugin LoadPlugin(string fileName)
        {
            var dataFile = Path.Combine(GetPluginsStoragePath(), fileName);

            var plugin = GetLoadSerializer().Deserialize<Plugin>(File.ReadAllBytes(dataFile));

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

        private void LoadPlugins()
        {
            LoadTypesCharacteristics();
            LoadPreviewNotePlayers();
            _globalService.Plugins.Clear();

            var pluginFiles = GetStoredPluginFiles();

            foreach (var filename in pluginFiles)
            {
                _globalService.Plugins.Add(LoadPlugin(filename));
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
    }
}