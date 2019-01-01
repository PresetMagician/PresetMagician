using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using SQLite;
using Anotar.Catel;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models;
using GSF;
using Type = System.Type;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia : AbstractVendorPresetParser
    {
        protected SQLiteConnection db;

        public void ScanPresets(List<string> instruments)
        {
            LogTo.Debug($"Attempting to load arturia presets using {GetDatabasePath()}");
            try
            {
                db = new SQLiteConnection(GetDatabasePath());
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
                var presetBank = new PresetBank();
                presetBank.BankName = pack.Pack.Key;

                foreach (var presetData in pack.Presets)
                {
                    var preset = new Preset();
                    preset.PresetBank = presetBank;
                    preset.PresetName = presetData.Preset.name;
                    preset.Author = presetData.SoundDesigner.name;
                    preset.Comment = presetData.Preset.comment;

                    var types = new ObservableCollection<string>();
                    types.Add(presetData.Type.name);

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


            db.Close();
        }

        private List<(Models.Preset Preset, Models.SoundDesigner SoundDesigner, Pack Pack, Models.Type Type)>
            GetPresets(List<Instrument> instruments)
        {
            var instrumentIds = (from q in instruments
                select q.key_id).ToList();

            return (from preset in db.Table<Models.Preset>()
                    join soundDesigner in db.Table<Models.SoundDesigner>() on preset.sound_designer equals soundDesigner
                        .key_id
                    join pack in db.Table<Models.Pack>() on preset.pack equals pack
                        .key_id
                    join type in db.Table<Models.Type>() on preset.type equals type
                        .key_id
                    where
                        preset.hide_in_browser == false &&
                        instrumentIds.Contains(preset.instrument_key)
                    select (preset, soundDesigner, pack, type)
                ).ToList();
        }

        private List<Models.Characteristic> GetPresetCharacteristics(Models.Preset preset)
        {
            var characteristicKeys= (from presetCharacteristic in db.Table<Models.PresetCharacteristic>()
                where presetCharacteristic.preset_key == preset.key_id
                select presetCharacteristic.characteristic_key).ToList();
            
            return (from characteristic in db.Table<Models.Characteristic>()
                    where characteristicKeys.Contains(characteristic.key_id)
                        select characteristic).ToList();
                    
                    
        }

        private List<Instrument> GetInstruments(List<string> instruments)
        {
            return db.Table<Instrument>().Where(o => instruments.Contains(o.name)).ToList();
        }

        private static string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Arturia\Presets\db.db3");
        }
    }
}