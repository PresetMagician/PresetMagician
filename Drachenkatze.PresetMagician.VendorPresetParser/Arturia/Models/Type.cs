using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Types", WithoutRowId = true)]
    public class Type
    {
        public int key_id { get; set; }
        public string name { get; set; }
    }
}