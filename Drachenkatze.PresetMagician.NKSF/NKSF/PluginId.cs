using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class PluginId
    {
        [Key("VST.magic")]
        public int VSTMagic;
    }
}