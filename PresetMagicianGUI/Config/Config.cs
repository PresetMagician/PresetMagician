using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresetMagicianGUI.Config
{
    public class ApplicationConfiguration
    {
        public VstPath[] VstPaths { get; set; }
    }

    public class VstPath
    {
        public string Path { get; set; }
    }
}
