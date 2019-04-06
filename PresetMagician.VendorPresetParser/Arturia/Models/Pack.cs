using SQLite;

namespace PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Packs", WithoutRowId = true)]
    internal class Pack
    {
        public int key_id { get; set; }
        public string name { get; set; }
    }
}