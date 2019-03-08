using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;

using System.IO.Compression;
using K4os.Compression.LZ4;
using SQLite;

namespace PresetMagicianScratchPad
{
  
    [SQLite.Table("PresetData", WithoutRowId = true)]
    public class PresetDataStorage
    {
        [PrimaryKey] public string PresetDataId { get; set; }

        private byte[] _compressedPresetDataCache;

        public bool IsCompressed { get; set; }

        [SQLite.Column("CompressedPresetData")]
        public byte[] CompressedPresetData
        {
            get
            {
                if (!IsCompressed || PresetData == null)
                {
                    return new byte[0];
                }

                if (_compressedPresetDataCache == null)
                {
                    _compressedPresetDataCache = LZ4Pickler.Pickle(PresetData);
                }

                return _compressedPresetDataCache;
            }
            set
            {
                if (!IsCompressed)
                {
                    return;
                }

                PresetData = LZ4Pickler.Unpickle(value);
            }
        }

        [SQLite.Column("PresetData")]
        public byte[] UncompressedPresetData
        {
            get
            {
                if (IsCompressed)
                {
                    return new byte[0];
                }

                return PresetData;
            }
            set
            {
                if (IsCompressed)
                {
                    return;
                }

                PresetData = value;
            }
        }

        [Ignore] public byte[] PresetData { get; set; }
    }
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            List<string> filenames = new List<string>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var db = new SQLiteConnection("test.sqlite3");
            db.CreateTable<PresetDataStorage>();		
            
            var source = @"C:\VstPlugins\u-he\ACE.data\Presets\ACE\BT beloved, too (vel).h2p";
           
           
                for (var i = 0; i < 10000; i++)
                {
                    
                    var fn = Guid.NewGuid().ToString();
                    filenames.Add(fn);
                    
                    var f = new PresetDataStorage();
                    f.IsCompressed = true;
                    f.PresetData = File.ReadAllBytes(source);
                    f.PresetDataId = fn;

                    db.InsertOrReplace(f);
                }
                
                db.Close();
            
            
            Debug.WriteLine("10k create: "+stopWatch.ElapsedMilliseconds);
stopWatch.Restart();
 db = new SQLiteConnection("test.sqlite3");		

            var rnd = new Random();
             
                for (var i = 0; i < 10000; i++)
                {
                    var fn = filenames[rnd.Next(10000)];
                    var entry = db.Get<PresetDataStorage>(fn);
                }
            
            
            Debug.WriteLine("10k random get: "+stopWatch.ElapsedMilliseconds);

            Debug.WriteLine("memory usage:"+Process.GetCurrentProcess().PrivateMemorySize64);
        }
        
    }
}