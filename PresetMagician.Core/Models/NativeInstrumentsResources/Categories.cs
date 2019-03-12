using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace PresetMagician.Core.Models.NativeInstrumentsResources
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Categories
    {
        [JsonProperty] public string Vendor { get; set; }

        [JsonProperty] public string Product { get; set; }
        public ObservableCollection<Category> CategoryNames { get; } = new ObservableCollection<Category>();

        [JsonProperty] public List<CategoryDB> CategoryDB { get; set; } = new List<CategoryDB>();
    }
}