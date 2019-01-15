using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Catel.IO;
using Drachenkatze.PresetMagician.Utils;
using K4os.Compression.LZ4;

namespace SharedModels
{
    public class PresetDataStorage
    {
        [Key]
        public string PresetDataStorageId { get; set; }

        private byte[] _compressedPresetDataCache;
        
        public bool IsCompressed { get; set; }
        [Column("CompressedPresetData", TypeName="blob")]
        public byte[] CompressedPresetData {
            get
            {
                if (!IsCompressed)
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

        [Column("PresetData", TypeName = "blob")]
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

        [NotMapped]
        public byte[] PresetData { get; set; }
    }
}