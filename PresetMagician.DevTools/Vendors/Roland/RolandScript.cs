using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using CsvHelper;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandScript: RolandStruct
    {
        public RolandScript(XElement node) : base(null, node)
        {
            StartAddress = 0;
            FileAddress = 0;

        }

        public void ConvertToCsv(string outputFile)
        {
            var structs = GetStructureData();
            
            using (var writer = new StreamWriter(outputFile))
            using (var csv = new CsvWriter(writer))
            {    
                csv.WriteRecords(structs);
            }
        }

        public override void Parse()
        {
            
            foreach (var childElement in SourceNode.Elements().ToList())
            {
                switch (childElement.Name.ToString())
                {
                    case "struct":
                        ApplyFoo(childElement, false);
                        break;
                }
            }
            
           
            
        }
    }
}