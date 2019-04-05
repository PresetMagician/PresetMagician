using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Utils;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        public string GetPresetsStorageFile(Plugin plugin)
        {
            return Path.Combine(GetPluginsStoragePath(), GetPluginStorageFilePrefix(plugin) + "."+plugin.PluginId + PresetStorageExtension);
        }
        
        /// <summary>
        /// Cleans up old preset storage files
        /// </summary>
        /// <param name="plugin"></param>
        public void CleanOldPresetStorageFiles(Plugin plugin)
        {
            var currentStorageFile = GetPresetsStorageFile(plugin);

            foreach (var file in GetStoredPresetFiles())
            {
                if (file.Contains(plugin.PluginId) && file != currentStorageFile)
                {
                    File.Move(file, file+".old");
                }
            }
        }
        
        public List<string> GetStoredPresetFiles()
        {
            var list = new List<string>();

            foreach (var file in Directory.EnumerateFiles(
                GetPluginsStoragePath(), "*" + PresetStorageExtension, SearchOption.AllDirectories))
            {
                list.Add(file);
            }

            return list;
        }
        
        public void SavePresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            plugin.OnBeforeCerasSerialize();
            var data = GetSaveSerializer().Serialize(plugin.Presets);

            File.WriteAllBytes(presetsStorageFile, data);

            SaveTypesCharacteristics();
            CleanOldPresetStorageFiles(plugin);
        }

        public async Task LoadPresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            if (File.Exists(presetsStorageFile))
            {
                var presets = GetLoadSerializer()
                    .Deserialize<EditableCollection<Preset>>(await AsyncFile.ReadAllBytesAsync(presetsStorageFile));

                foreach (var preset in presets)
                {
                    preset.Plugin = plugin;
                }
                plugin.Presets = presets;
                plugin.OnAfterCerasDeserialize();
            }
        }
    }
}