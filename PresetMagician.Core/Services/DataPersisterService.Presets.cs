using System.IO;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;

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

            var data = GetSaveSerializer().Serialize(plugin.Presets);

            File.WriteAllBytes(presetsStorageFile, data);

            SaveTypesCharacteristics();
        }

        public void LoadPresetsForPlugin(Plugin plugin)
        {
            var presetsStorageFile = GetPresetsStorageFile(plugin);

            if (File.Exists(presetsStorageFile))
            {
                var presets = GetLoadSerializer()
                    .Deserialize<EditableCollection<Preset>>(File.ReadAllBytes(presetsStorageFile));
                plugin.Presets = presets;
            }
        }
    }
}