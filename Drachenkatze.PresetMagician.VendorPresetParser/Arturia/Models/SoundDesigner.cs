using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Sound_Designers", WithoutRowId = true)]
    internal class SoundDesigner
    {
        public int key_id { get; set; }
        public string name { get; set; }
    }
}