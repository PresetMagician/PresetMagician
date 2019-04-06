using SQLite;

namespace PresetMagician.VendorPresetParser.Arturia.Models
{
    [Table("Instruments", WithoutRowId = true)]
    internal class Instrument
    {
        public int key_id { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public int category { get; set; }
    }
}