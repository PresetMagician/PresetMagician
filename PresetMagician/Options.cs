using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.GUI
{
    class Options
    {
        [Option(Default = false,Hidden=true)]
        public bool ForceRegistration { get; set; }
    }
}
