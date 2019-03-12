using K4os.Compression.LZ4;
using SQLite;

namespace PresetMagician.Core.Models
{
    [Table("PresetData", WithoutRowId = true)]
    public class PresetDataStorage
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [PrimaryKey] public string PresetDataId { get; set; }

        private byte[] _compressedPresetDataCache;

        [Ignore] public int PresetCompressedSize
        {
            get
            {
                if (_compressedPresetDataCache == null)
                {
                    return 0;
                }
                return _compressedPresetDataCache.Length;
            }
        }

        [Column("CompressedPresetData")]
        // ReSharper disable once UnusedMember.Global
        public byte[] CompressedPresetData
        {
            get
            {
                if (PresetData == null)
                {
                    return new byte[0];
                }

                if (_compressedPresetDataCache == null)
                {
                    _compressedPresetDataCache = LZ4Pickler.Pickle(PresetData);
                }

                return _compressedPresetDataCache;
            }
            set => PresetData = LZ4Pickler.Unpickle(value);
        }

        [Ignore] public byte[] PresetData { get; set; }
    }
}