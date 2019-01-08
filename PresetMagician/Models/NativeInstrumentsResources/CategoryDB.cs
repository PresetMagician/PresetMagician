using System.Collections.Generic;
using Newtonsoft.Json;

namespace PresetMagician.Models.NativeInstrumentsResources
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CategoryDB
    {
        [JsonProperty]
        public string FileType { get; set; }
        [JsonProperty]
        public List<string> Categories { get; set; } = new List<string>();
    }
}