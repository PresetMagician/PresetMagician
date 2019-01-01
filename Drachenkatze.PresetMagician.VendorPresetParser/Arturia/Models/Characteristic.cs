using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Characteristics", WithoutRowId = true)]
    public class Characteristic
    {
        public int key_id { get; set; }
        public string name { get; set; }
    }
}