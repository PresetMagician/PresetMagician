using System.Collections.Generic;
using System.IO;
using System.Linq;
using PresetMagician.Core.Models;
using PresetMagician.Utils;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        /// <summary>
        /// Returns the full path to where the plugins are stored
        /// </summary>
        /// <returns></returns>
        public string GetPluginsStoragePath()
        {
            var pluginsStoragePath = Path.Combine(DefaultDataStoragePath, DefaultPluginStoragePath);
            Directory.CreateDirectory(pluginsStoragePath);
            return pluginsStoragePath;
        }

        /// <summary>
        /// Returns the full storage path for a plugin
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public string GetPluginStorageFile(Plugin plugin)
        {
            return Path.Combine(GetPluginsStoragePath(), GetPluginStorageFilePrefix(plugin) + "."+ plugin.PluginId +  PluginStorageExtension);
        }

        public string GetPluginStorageFilePrefix(Plugin plugin)
        {
            return PathUtils.SanitizeFilename(plugin.PluginVendor + " - " + plugin.PluginName);
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

        /// <summary>
        /// Cleans up old plugin storage files
        /// </summary>
        /// <param name="plugin"></param>
        public void CleanOldPluginStorageFiles(Plugin plugin)
        {
            var currentStorageFile = GetPluginStorageFile(plugin);

            foreach (var file in GetStoredPluginFiles())
            {
                if (file.Contains(plugin.PluginId) && file != currentStorageFile)
                {
                    File.Move(file, file+".old");
                }
            }

        }
        
        /// <summary>
        /// Saves a plugin including characteristics, modes and preview note players. Does not include presets! 
        /// </summary>
        /// <param name="plugin"></param>
        public void SavePlugin(Plugin plugin)
        {
            Directory.CreateDirectory(DefaultPluginStoragePath);

            var dataFile = GetPluginStorageFile(plugin);

            var data = GetSaveSerializer().Serialize(plugin);

            File.WriteAllBytes(dataFile, data);

            SaveTypesCharacteristics();
            SavePreviewNotePlayers();

            CleanOldPluginStorageFiles(plugin);
        }


        /// <summary>
        /// Loads a plugin by it's filename
        /// </summary>
        /// <param name="fileName">An absolute filename or a relative one. Needs to include the extension.</param>
        /// <returns></returns>
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