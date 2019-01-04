using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Anotar.Catel;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models;
using GSF;
using SQLite;
using Environment = System.Environment;
using Exception = System.Exception;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia : AbstractVendorPresetParser
    {
        private SQLiteConnection _db;

        protected void ScanPresets(List<string> instruments)
        {
            LogTo.Debug($"Attempting to load arturia presets using {GetDatabasePath()}");
            try
            {
                _db = new SQLiteConnection(GetDatabasePath());
            }
            catch (Exception e)
            {
                LogTo.Debug(e.ToString);
                return;
            }


            var instrumentList = GetInstruments(instruments);
            var allPresets = GetPresets(instrumentList);

            var presetsByPacks = (from p in allPresets
                group p by p.Pack.name
                into g
                select new {Pack = g, Presets = g.ToList()}).ToList();

            foreach (var pack in presetsByPacks)
            {
                var presetBank = new PresetBank {BankName = pack.Pack.Key};

                foreach (var presetData in pack.Presets)
                {
                    var preset = new Preset
                    {
                        PresetBank = presetBank,
                        PresetName = presetData.Preset.name,
                        Author = presetData.SoundDesigner.name,
                        Comment = presetData.Preset.comment
                    };

                    var types = new ObservableCollection<string> {presetData.Type.name};

                    preset.Types.Add(types);

                   var characteristics = GetPresetCharacteristics(presetData.Preset);
                   
                   
                   preset.Modes.AddRange((from c in characteristics select c.name).ToList());
                   preset.SetPlugin(VstPlugin);

                   var fileName = presetData.Preset.file_path.Replace('/', '\\');
                   var content = File.ReadAllBytes(fileName);
                   var ms = new MemoryStream();
                   ms.Write(LittleEndian.GetBytes(content.Length),0,4);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.Write(LittleEndian.GetBytes(content.Length),0,4);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.WriteByte(0);
                   ms.Write(content, 0, content.Length);
                   ms.Write(content, 0, content.Length);

                   preset.PresetData = ms.ToArray();
                   Presets.Add(preset);
                }

                RootBank.PresetBanks.Add(presetBank);
            }

            Debug.WriteLine(presetsByPacks);


            _db.Close();
        }

        private IEnumerable<(Models.Preset Preset, SoundDesigner SoundDesigner, Pack Pack, Type Type)> GetPresets(IEnumerable<Instrument> instruments)
        {
            var instrumentIds = (from q in instruments
                select q.key_id).ToList();

            return (from preset in _db.Table<Models.Preset>()
                    join soundDesigner in _db.Table<SoundDesigner>() on preset.sound_designer equals soundDesigner
                        .key_id
                    join pack in _db.Table<Pack>() on preset.pack equals pack
                        .key_id
                    join type in _db.Table<Type>() on preset.type equals type
                        .key_id
                    where
                        preset.hide_in_browser == false &&
                        instrumentIds.Contains(preset.instrument_key)
                    select (preset, soundDesigner, pack, type)
                ).ToList();
        }

        private IEnumerable<Characteristic> GetPresetCharacteristics(Models.Preset preset)
        {
            var characteristicKeys= (from presetCharacteristic in _db.Table<PresetCharacteristic>()
                where presetCharacteristic.preset_key == preset.key_id
                select presetCharacteristic.characteristic_key).ToList();
            
            return (from characteristic in _db.Table<Characteristic>()
                    where characteristicKeys.Contains(characteristic.key_id)
                        select characteristic).ToList();
                    
                    
        }

        private IEnumerable<Instrument> GetInstruments(ICollection<string> instruments)
        {
            return _db.Table<Instrument>().Where(o => instruments.Contains(o.name)).ToList();
        }

        private static string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Arturia\Presets\db.db3");
        }
    }
}