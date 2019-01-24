using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel.Collections;
using GSF;
using SharedModels;
using SQLite;
using ArturiaModels = Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public abstract class Arturia : AbstractVendorPresetParser
    {
        private SQLiteConnection _db;

        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {GetDatabasePath()}";
        
        protected abstract List<string> GetInstrumentNames();

        public override int GetNumPresets()
        {
            InitDb();
            var instruments = GetInstrumentNames();
            var instrumentList = GetInstruments(instruments);

            return GetPresets(instrumentList).Count();
        }

        private void InitDb()
        {
            if (_db != null)
            {
                return;
            }

            Plugin.Debug($"Attempting to load arturia presets using {GetDatabasePath()}");
            _db = new SQLiteConnection(GetDatabasePath());
        }

        public override async Task DoScan()
        {
            InitDb();
            var instruments = GetInstrumentNames();

            var instrumentList = GetInstruments(instruments);
            var allPresets = GetPresets(instrumentList);

            var presetsByPacks = (from p in allPresets
                group p by p.Pack.name
                into g
                select new {Pack = g, Presets = g.ToList()}).ToList();

            foreach (var pack in presetsByPacks)
            {
                var presetBank = RootBank.CreateRecursive(pack.Pack.Key);
                
                foreach (var presetData in pack.Presets)
                {
                    var preset = new Preset
                    {
                        PresetBank = presetBank,
                        PresetName = presetData.Preset.name,
                        Author = presetData.SoundDesigner.name,
                        Comment = presetData.Preset.comment,
                        Plugin = Plugin
                    };

                    var types = new ObservableCollection<string> {presetData.Type.name};

                    preset.Types.Add(types);

                    var characteristics = GetPresetCharacteristics(presetData.Preset);


                    preset.Modes.AddRange((from c in characteristics select c.name).ToList());
                    preset.SetPlugin(Plugin);

                    var fileName = presetData.Preset.file_path.Replace('/', '\\');
                    var content = File.ReadAllBytes(fileName);
                    var ms = new MemoryStream();
                    ms.Write(LittleEndian.GetBytes(content.Length), 0, 4);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.Write(LittleEndian.GetBytes(content.Length), 0, 4);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                    ms.Write(content, 0, content.Length);
                    ms.Write(content, 0, content.Length);

                    preset.SourceFile = fileName;
                    await PresetDataStorer.PersistPreset(preset, ms.ToArray());
                }
            }

            _db.Close();
        }

        private IEnumerable<(ArturiaModels.Preset Preset, ArturiaModels.SoundDesigner SoundDesigner, ArturiaModels.Pack
            Pack,
            ArturiaModels.Type Type)> GetPresets(IEnumerable<ArturiaModels.Instrument> instruments)
        {
            var instrumentIds = (from q in instruments
                select q.key_id).ToList();

            return (from preset in _db.Table<ArturiaModels.Preset>()
                    join soundDesigner in _db.Table<ArturiaModels.SoundDesigner>() on preset.sound_designer equals
                        soundDesigner
                            .key_id
                    join pack in _db.Table<ArturiaModels.Pack>() on preset.pack equals pack
                        .key_id
                    join type in _db.Table<ArturiaModels.Type>() on preset.type equals type
                        .key_id
                    where
                        preset.hide_in_browser == false &&
                        instrumentIds.Contains(preset.instrument_key)
                    select (preset, soundDesigner, pack, type)
                ).ToList();
        }

        private IEnumerable<ArturiaModels.Characteristic> GetPresetCharacteristics(ArturiaModels.Preset preset)
        {
            var characteristicKeys = (from presetCharacteristic in _db.Table<ArturiaModels.PresetCharacteristic>()
                where presetCharacteristic.preset_key == preset.key_id
                select presetCharacteristic.characteristic_key).ToList();

            return (from characteristic in _db.Table<ArturiaModels.Characteristic>()
                where characteristicKeys.Contains(characteristic.key_id)
                select characteristic).ToList();
        }

        private IEnumerable<ArturiaModels.Instrument> GetInstruments(ICollection<string> instruments)
        {
            return _db.Table<ArturiaModels.Instrument>().Where(o => instruments.Contains(o.name)).ToList();
        }

        private static string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Arturia\Presets\db.db3");
        }
    }
}