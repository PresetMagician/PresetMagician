using System.ComponentModel.DataAnnotations;

namespace PresetMagician.Legacy.Models
{
    public class SchemaVersion
    {
        [Key] public string Version { get; set; }
    }
}