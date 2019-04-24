using Newtonsoft.Json;

namespace PresetMagician.VendorPresetParser.RevealSound.Internal
{
    public class SpireJsonConfig
    {
        [JsonProperty("selected_bank")] 
        public string SelectedBank { get; set; }
    }
}