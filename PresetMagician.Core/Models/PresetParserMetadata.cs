using Ceras;

namespace PresetMagician.Core.Models
{
    public class PresetParserMetadata : PresetMetadata
    {
        [Include] public string SourceFile { get; set; }
        public Plugin Plugin { get; set; }
    }
}