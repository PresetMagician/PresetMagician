using System.ComponentModel.DataAnnotations;

namespace SharedModels
{
    public class SchemaVersion
    {
        [Key] public string Version { get; set; }
    }
}