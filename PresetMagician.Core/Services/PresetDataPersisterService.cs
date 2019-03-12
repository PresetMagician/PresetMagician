using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using SQLite;

namespace PresetMagician.Core.Services
{
    public class PresetDataPersisterService: IDataPersistence
    {
        private SQLiteAsyncConnection _db;
        
        public static string DefaultDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PresetMagicianPresets.sqlite3");

        
        public async Task OpenDatabase()
        {
            if (_db == null)
            {
                _db = new SQLiteAsyncConnection(DefaultDatabasePath);
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
        
        public async Task PersistPreset(Preset preset, byte[] data)
        {
            var existingPreset = (from p in preset.Plugin.Presets where p.SourceFile == preset.SourceFile select p)
                .FirstOrDefault();

            if (existingPreset != null)
            {
                existingPreset.SetFromPresetParser(preset);
                preset = existingPreset;
            }
            else
            {
                preset.Plugin.Presets.Add(preset);
            }

            var hash = HashUtils.getIxxHash(data);

            if (hash == preset.PresetHash && preset.PresetSize == data.Length)
            {
                return;
            }
            preset.PresetHash = hash;
            preset.PresetSize = data.Length;
            
            var presetData = new PresetDataStorage {PresetData = data, PresetDataId = preset.PresetId};
            await _db.InsertOrReplaceAsync(presetData);
            preset.PresetCompressedSize = presetData.PresetCompressedSize;
        }
        
        public byte[] GetPresetData(Preset preset)
        {
            var data = _db.GetAsync<PresetDataStorage>(preset.PresetId).Result;

            return data.PresetData;
           
        }
    }
}