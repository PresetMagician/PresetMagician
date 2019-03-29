using SQLite;

namespace PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Types", WithoutRowId = true)]
    internal class Type
    {
        public int key_id { get; set; }
        public string name { get; set; }
    }
}