using SQLite;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Instruments", WithoutRowId = true)]
    public class Instrument
    {
        public int key_id { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public int category { get; set; }
    }
}