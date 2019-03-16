using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using SQLite;

namespace PresetMagician.Core.Services
{
    public class PresetDataPersisterService: IDataPersistence
    {
        public event EventHandler<PresetUpdatedEventArgs> PresetUpdated;
        private SQLiteAsyncConnection _db;
        
        public static string DefaultDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PresetMagicianPresets.sqlite3");

        
        public async Task OpenDatabase()
        {
            if (_db == null)
            {
                var dbDir = Path.GetDirectoryName(DefaultDatabasePath);
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }
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
        
        public async Task PersistPreset(PresetParserMetadata presetMetadata, byte[] data)
        {
            var plugin = presetMetadata.Plugin;
            var preset = (from p in plugin.Presets where p.OriginalMetadata.SourceFile == presetMetadata.SourceFile select p)
                .FirstOrDefault();

            if (preset == null)
            {
                preset = new Preset();
                plugin.Presets.Add(preset);
            }
           
            preset.SetFromPresetParser(presetMetadata);
            
            PresetUpdated?.Invoke(this, new PresetUpdatedEventArgs(preset));

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