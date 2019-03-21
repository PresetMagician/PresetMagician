using System;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace PresetMagician.Core.Models.NativeInstrumentsResources
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Color : ModelBase
    {
        [JsonProperty] public string VB_bgcolor { get; set; }
        [IncludeInSerialization] public System.Windows.Media.Color BackgroundColor { get; set; }

        public void SetRandomColor()
        {
            BackgroundColor = GetRandomColor();
        }

        public static System.Windows.Media.Color GetRandomColor()
        {
            Random r = new Random();

            var color = new System.Windows.Media.Color
            {
                A = 255, R = (byte) r.Next(0, 127), G = (byte) r.Next(0, 127), B = (byte) r.Next(0, 127)
            };

            return color;
        }
    }
}