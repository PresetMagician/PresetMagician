using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using K4os.Compression.LZ4;

namespace PresetMagician.Legacy.Models
{
    public class PresetDataStorage 
    {
        [Key] public string PresetDataStorageId { get; set; }

        public bool IsCompressed { get; set; }

        [Column("CompressedPresetData", TypeName = "blob")]
        public byte[] CompressedPresetData { get; set; }
    

        [Column("PresetData", TypeName = "blob")]
        public byte[] UncompressedPresetData { get; set; }
      

        [NotMapped]
        public byte[] PresetData
        {
            get
            {
                if (IsCompressed)
                {
                    if (CompressedPresetData == null)
                    {
                        return new byte[0];
                    }
                    else
                    {
                        return LZ4Pickler.Unpickle(CompressedPresetData);
                    }
                }
                else
                {
                    return UncompressedPresetData;
                }
            }
        }
    }
}