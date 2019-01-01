using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Preset_Characteristics", WithoutRowId = true)]
    public class PresetCharacteristic
    {
        public int preset_key { get; set; }
        public int characteristic_key { get; set; }
    }
}