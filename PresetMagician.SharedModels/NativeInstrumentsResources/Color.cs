using Catel.Data;
using Newtonsoft.Json;

namespace SharedModels.NativeInstrumentsResources
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Color : ModelBase
    {
        [JsonProperty] public string VB_bgcolor { get; set; }
        public System.Windows.Media.Color BackgroundColor { get; set; }
    }
}