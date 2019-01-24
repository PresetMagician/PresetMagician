using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PresetMagician.Models
{
    [DataContract]
    public class PluginInfoItem
    {
        [DataMember]
        public string Category { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string Value { get; set; }

        public PluginInfoItem(string category, string name, string value)
        {
            Category = category;
            Name = name;
            Value = value;
        }
    }
}
