using System;
using System.IO;
using System.Threading.Tasks;
using SharedModels.NewModels;
using SQLite;

namespace SharedModels.Services
{
    public class PresetDataPersisterService
    {
        private SQLiteAsyncConnection _db;
        
        public static string DefaultDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PresetMagicianPresets.sqlite3");

        public async Task OpenDatabase()
        {
            _db = new SQLiteAsyncConnection(DefaultDatabasePath);
            await _db.CreateTableAsync<PresetDataStorage>();	
        }

        public async Task CloseDatabase()
        {
            await _db.CloseAsync();
        }
        
        public async Task PersistPreset(Preset preset, byte[] data)
        {
            var presetData = new PresetDataStorage();
            presetData.PresetData = data;
            presetData.PresetDataId = preset.PresetId;
            await _db.InsertOrReplaceAsync(presetData);
        }
    }
}