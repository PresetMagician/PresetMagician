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
            return Path.Combine(GetPluginsStoragePath(), plugin.PluginId + PresetStorageExtension);
        }
        
        public void SavePresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            plugin.OnBeforeCerasSerialize();
            var data = GetSaveSerializer().Serialize(plugin.Presets);

            File.WriteAllBytes(presetsStorageFile, data);

            SaveTypesCharacteristics();
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