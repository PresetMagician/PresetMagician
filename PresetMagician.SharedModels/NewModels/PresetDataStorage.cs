using K4os.Compression.LZ4;
using SQLite;

namespace SharedModels.NewModels
{
    [SQLite.Table("PresetData", WithoutRowId = true)]
    public class PresetDataStorage
    {
        [PrimaryKey] public string PresetDataId { get; set; }

        private byte[] _compressedPresetDataCache;

        [SQLite.Column("CompressedPresetData")]
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