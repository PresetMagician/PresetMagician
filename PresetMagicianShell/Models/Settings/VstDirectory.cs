using System;
using Newtonsoft.Json;

namespace PresetMagicianShell.Models.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VstDirectory
    {
        [JsonProperty]
        public string Path { get; set; }
        [JsonProperty]
        public bool Active { get; set; } = true;
    }
}