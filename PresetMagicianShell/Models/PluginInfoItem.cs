using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PresetMagicianShell.Models
{
    public class PluginInfoItem : ModelBase
    {
        public string Title { get; set; }

        public PluginInfoItem(string title)
        {
            Title = title;
        }
    }
}
