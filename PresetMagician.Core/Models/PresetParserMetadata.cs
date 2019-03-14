using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Ceras;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Models
{

    public class PresetParserMetadata : PresetMetadata
    {
        [Include] public string SourceFile { get; set; }
        public Plugin Plugin { get; set; }
    }
}