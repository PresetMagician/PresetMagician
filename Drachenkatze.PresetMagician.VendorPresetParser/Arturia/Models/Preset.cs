using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Preset_Id", WithoutRowId = true)]
    public class Preset
    {
        public int key_id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public int instrument_key { get; set; }
        public int sound_designer { get; set; }
        public int pack { get; set; }
        public string file_path { get; set; }
        public string comment { get; set; }
        public bool hide_in_browser { get; set; }
    }
}