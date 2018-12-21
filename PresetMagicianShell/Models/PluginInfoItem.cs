using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PresetMagicianShell.Models
{
    public class PluginInfoItem
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public PluginInfoItem(string category, string name, string value)
        {
            Category = category;
            Name = name;
            Value = value;
        }
    }
}
