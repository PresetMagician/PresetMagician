using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using SQLite;

namespace PresetMagician.Core.Services
{
    public class PresetDataPersisterService : IDataPersistence
    {
        public event EventHandler<PresetUpdatedEventArgs> PresetUpdated;
        private SQLiteAsyncConnection _db;

        public static string DefaultDatabaseFile = "PresetMagicianPresets.sqlite3";

        public static string GetDatabaseFile()
        {
            Directory.CreateDirectory(DataPersisterService.DefaultDataStoragePath);
            return Path.Combine(DataPersisterService.DefaultDataStoragePath, DefaultDatabaseFile);
        }

        public async Task OpenDatabase()
        {
            if (_db == null)
            {
               
                _db = new SQLiteAsyncConnection(GetDatabaseFile());
                await _db.CreateTableAsync<PresetDataStorage>();
            }
        }

        public async Task CloseDatabase()
        {
            if (_db != null)
            {
                await _db.CloseAsync();
                _db = null;
            }
        }

        public async Task PersistPreset(PresetParserMetadata presetMetadata, byte[] data, bool force = false)
        {
            var plugin = presetMetadata.Plugin;
            var preset = (from p in plugin.Presets
                    where p.OriginalMetadata.SourceFile == presetMetadata.SourceFile
                    select p)
                .FirstOrDefault();

            if (preset == null)
            {
                preset = new Preset();
                plugin.Presets.Add(preset);
            }

            preset.SetFromPresetParser(presetMetadata);

            PresetUpdated?.Invoke(this, new PresetUpdatedEventArgs(preset));

            var hash = HashUtils.getIxxHash(data);

            if (hash == preset.PresetHash && preset.PresetSize == data.Length && !force)
            {
                return;
            }

            preset.PresetHash = hash;
            preset.PresetSize = data.Length;

            var presetData = new PresetDataStorage {PresetData = data, PluginId = preset.Plugin.PluginId, PresetDataId = preset.PresetId};
            await _db.InsertOrReplaceAsync(presetData);
            preset.PresetCompressedSize = presetData.PresetCompressedSize;
        }

        public async Task DeletePresetsForPlugin(Plugin plugin)
        {
            bool closeDb = false;
            if (_db == null)
            {
                await OpenDatabase();
                closeDb = true;
            }

            await _db.ExecuteAsync("DELETE FROM PresetData WHERE PluginId = ?", plugin.PluginId);
            
            if (closeDb)
            {
                await CloseDatabase();
            }
        }

        public async Task<byte[]> GetPresetData(Preset preset)
        {
            bool closeDb = false;
            if (_db == null)
            {
                await OpenDatabase();
                closeDb = true;
            }

            var data = await _db.GetAsync<PresetDataStorage>(preset.PresetId);

            if (closeDb)
            {
                await CloseDatabase();
            }
            return data.PresetData;
        }
    }
}