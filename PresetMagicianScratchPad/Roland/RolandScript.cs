using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Xml.Linq;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandScript: RolandStruct
    {
        public RolandScript(XElement node) : base(null, node)
        {
            StartAddress = 0;
            FileAddress = 0;
            ExportNode = new XElement("root");

        }
        
        public void PrepareCallbacks(string device)
        {
            switch (device)
            {
                case "D-50":
                    RegisterCallback("RolandValue.AfterParse",
                        RolandD50Callback);
                    break;
                case "JV-1080":
                    RegisterCallback("RolandValue.AfterParse",
                        JV1080Callback);
                    break;
                case "JX-3P":
                    RegisterCallback("RolandValue.AfterParse",
                        RolandJX3PCallback);
                    break;
                case "TR-808":
                    RegisterCallback("RolandValue.AfterParse",
                        RolandTR808Callback);
                    break;
                case "TR-909":
                    RegisterCallback("RolandValue.AfterParse",
                        RolandTR909Callback);
                    break;
            }   
        }
        
        static string RolandTR909GetInstName(int index)
        {
            switch (index)
            {
                case 0:
                    return "BASS DRUM";
                    break;
                case 1:
                    return "SNARE DRUM";
                    break;
                case 2:
                    return "LOW TOM";
                    break;
                case 3:
                    return "MID TOM";
                    break;
                case 4:
                    return "HI TOM";
                    break;
                case 5:
                    return "RIM";
                    break;
                case 6:
                    return "CLAP";
                    break;
                case 7:
                    return "CLOSED HIHAT";
                    break;
                case 8:
                    return "OPEN HIHAT";
                    break;
                case 9:
                    return "CRASH CYMBAL";
                    break;
                case 10:
                    return "RIDE CYMBAL";
                    break;
                case 11:
                    return "ACCENT";
                default:
                    throw new Exception("FOO?");
                  
            }
        }

        static string RolandTR808GetInstName(int index)
        {
            switch (index)
            {
                case 0:
                    return "BD";
                    break;
                case 1:
                    return "SD";
                    break;
                case 2:
                    return "LT/LC";
                    break;
                case 3:
                    return "MT/MC";
                    break;
                case 4:
                    return "HT/HC";
                    break;
                case 5:
                    return "RS/CL";
                    break;
                case 6:
                    return "CP/MA";
                    break;
                case 7:
                    return "CH";
                    break;
                case 8:
                    return "OH";
                    break;
                case 9:
                    return "CY";
                    break;
                case 10:
                    return "CB";
                    break;
                case 11:
                    return "ACCENT";
                default:
                    throw new Exception("FOO?");
                  
            }
        }

        static void RolandTR909Callback(RolandMemorySection section)
        {
            if (section.Parent.Name.StartsWith("instCommon"))
            {
                var sIndex = section.Parent.Name.IndexOf("[", StringComparison.InvariantCulture);
                var eIndex = section.Parent.Name.IndexOf("]", StringComparison.InvariantCulture);

                var numStr = section.Parent.Name.Substring(sIndex + 1, eIndex - sIndex-1);
                var num = int.Parse(numStr);
                var instName = RolandTR909GetInstName(num);
                

                if (instName != "")
                {
                    section.VstParameterName = section.VstParameterName.Replace("INST", instName);

                    if ((num == 1)
                        
                        && section.VstParameterName.Contains("TONE") && section.Type == "int4x4")
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TONE", "TONE2");
                    }
                    /*
                    if ((num == 9 || num == 1) && section.VstParameterName.Contains("TUNE"))
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TUNE", "TONE");
                    }*/
                    
                    if (section.VstParameterName.Contains("GAIN"))
                    {
                       
                            section.VstParameterName = section.VstParameterName.Replace("GAIN", "GAIN2");
                        
                    }

                    if (section.VstParameterName == "SNARE DRUM DECAY")
                    {
                        section.VstParameterName = "SNARE DRUM TONE";
                    }
                    /*
                    if (num >= 2 && num <=4 &&section.VstParameterName.Contains("TUNE"))
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TUNE", "TUNING");
                    }*/
                }
            }

            if (section.Parent.Name == "saveKit")
            {
                if (section.VstParameterName == "BD ATTACK")
                {
                    section.VstParameterName = "BASS DRUM ATTACK";
                }
                
                if (section.VstParameterName == "SD SNAPPY")
                {
                    section.VstParameterName = "SNARE DRUM SNAPPY";
                }
                
                if (section.VstParameterName.StartsWith("INST"))
                {
                    var instName = "";
                    var num = section.VstParameterName.Substring(4, 2);
                    if (int.TryParse(num, out int idx))
                    {
                        instName = RolandTR909GetInstName(idx);
                    }



                    if (instName != "")
                    {
                        section.VstParameterName = section.VstParameterName.Replace($"INST" + idx.ToString(), instName);

                        if (section.VstParameterName.Contains("BOOST"))
                        {
                            section.VstParameterName = section.VstParameterName.Replace("BOOST", "GAIN");
                        }
                    }
                }
            }

            if (section.Parent.Name == "ptnCmn")
            {
                if (section.VstParameterName == "INST02 SHUFFLE")
                {
                    section.VstParameterName = "SNARE SHUFFLE";
                }
                if (section.VstParameterName.StartsWith("INST"))
                {
                    var instName = "";
                    var num = section.VstParameterName.Substring(4, 2);
                    if (int.TryParse(num, out int idx))
                    {
                        instName = RolandTR909GetInstName(idx-1);
                    }



                    if (instName != "")
                    {
                        section.VstParameterName = section.VstParameterName.Replace("INST" + num.ToString(), instName);
                    }
                }

                if (section.VstParameterName == "ACCENT")
                {
                    section.VstParameterName = "ACCENT LEVEL";
                }
                
                if (section.VstParameterName == "SHUFFLE(COMMON)")
                {
                    section.VstParameterName = "SHUFFLE";
                }
            }
             
        }

        static void RolandTR808Callback(RolandMemorySection section)
        {
            if (section.Parent.Name.StartsWith("instCommon"))
            {
                var sIndex = section.Parent.Name.IndexOf("[", StringComparison.InvariantCulture);
                var eIndex = section.Parent.Name.IndexOf("]", StringComparison.InvariantCulture);

                var numStr = section.Parent.Name.Substring(sIndex + 1, eIndex - sIndex-1);
                var num = int.Parse(numStr);
                var instName = RolandTR808GetInstName(num);
                

                if (instName != "")
                {
                    section.VstParameterName = section.VstParameterName.Replace("INST", instName);

                    if ((num == 9 || num == 0 || num == 1)
                        
                        && section.VstParameterName.Contains("TONE") && section.Type == "int4x4")
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TONE", "TONE2");
                    }
                    if ((num == 9 || num == 1) && section.VstParameterName.Contains("TUNE"))
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TUNE", "TONE");
                    }
                    
                    if (section.VstParameterName.Contains("GAIN"))
                    {
                       
                            section.VstParameterName = section.VstParameterName.Replace("GAIN", "GAIN2");
                        
                    }
                    if (num >= 2 && num <=4 &&section.VstParameterName.Contains("TUNE"))
                    {
                        section.VstParameterName = section.VstParameterName.Replace("TUNE", "TUNING");
                    }
                }
            }

            if (section.Parent.Name == "saveKit")
            {
                if (section.VstParameterName == "BD ATTACK")
                {
                    section.VstParameterName = "BD TONE";
                }
                
                if (section.VstParameterName.StartsWith("INST"))
                {
                    var instName = "";
                    var num = section.VstParameterName.Substring(4, 2);
                    if (int.TryParse(num, out int idx))
                    {
                        instName = RolandTR808GetInstName(idx);
                    }



                    if (instName != "")
                    {
                        section.VstParameterName = section.VstParameterName.Replace($"INST" + idx.ToString(), instName);

                        if (section.VstParameterName.Contains("BOOST"))
                        {
                            section.VstParameterName = section.VstParameterName.Replace("BOOST", "GAIN");
                        }
                    }
                }
            }

            if (section.Parent.Name == "ptnCmn")
            {

                if (section.VstParameterName.StartsWith("INST"))
                {
                    var instName = "";
                    var num = section.VstParameterName.Substring(4, 2);
                    if (int.TryParse(num, out int idx))
                    {
                        instName = RolandTR808GetInstName(idx-1);
                    }



                    if (instName != "")
                    {
                        section.VstParameterName = section.VstParameterName.Replace("INST" + num.ToString(), instName);
                    }
                }

                if (section.VstParameterName == "ACCENT")
                {
                    section.VstParameterName = "ACCENT LEVEL";
                }
                
                if (section.VstParameterName == "SHUFFLE(COMMON)")
                {
                    section.VstParameterName = "SHUFFLE";
                }
            }
             
            
        }
        
        static void RolandJX3PCallback(RolandMemorySection section)
        {
            if (section.VstParameterName == "VCF CUTOFF FREQ")
            {
                section.VstParameterName = "VCF CUTOFF";
            }

            if (section.VstParameterName == "ASSIGN MODE")
            {
                section.VstParameterName = "KEY ASSIGN";
            }
        }

        static void JV1080Callback(RolandMemorySection section)
        {
            var parentType = section.Parent.Type;
                
            
            if (parentType == "PatchCommonMFX" || parentType == "PatchCommonChorus" || parentType == "PatchCommonReverb" )
            {
                string prefix = "";
                switch (section.Parent.Parent.Type)
                {
                    case "Rhythm":
                        prefix = "RHYTHM ";
                        break;
                    case "Patch":
                        prefix = "PATCH ";
                        break;
                }

                section.VstParameterName = prefix + section.VstParameterName;
            }

            if (parentType == "PatchTone")
            {
                string prefix = "";
                switch (section.Parent.Offset)
                {
                    case 0x00001000:
                        prefix = "TONE1 ";
                        break;
                    case 0x00001100:
                        prefix = "TONE2 ";
                        break;
                    case 0x00001200:
                        prefix = "TONE3 ";
                        break;
                    case 0x00001300:
                        prefix = "TONE4 ";
                        break;
                    
                }

                section.VstParameterName = prefix + section.VstParameterName;
                /*var s = section.Parent;
                
                Debug.WriteLine($" {s.GetTypeName()} {s.Name} 0x{s.Offset:x8}");*/

            }
            if (parentType == "RhythmKey")
            {
                var idx = ((section.Parent.Offset - 2048) / 256) + 1;
                string prefix = $"RHYTHM KEY{idx} ";

                
               
                section.VstParameterName = prefix + section.VstParameterName;
                /*var s = section.Parent;
                
                Debug.WriteLine($" {s.GetTypeName()} {s.Name} 0x{s.Offset:x8}");*/

            }
        }

        static void RolandD50Callback(RolandMemorySection section)
        {
           
            if (section.Parent.Name.StartsWith("PARTIAL"))
            {
                string prefix = "";
                switch (section.Parent.Offset)
                {
                    case 0x00000000:
                        prefix = "UPPER1 ";
                        break;
                    case 0x00000040:
                        prefix = "UPPER2 ";
                        break;
                    case 0x000000c0:
                        prefix = "LOWER1 ";
                        break;
                    case 0x00000100:
                        prefix = "LOWER2 ";
                        break;
                                
                }

                section.VstParameterName = prefix + section.VstParameterName;
            }
                    
            if (section.Parent.Name.StartsWith("TONE"))
            {
                string prefix = "";
                switch (section.Parent.Offset)
                {
                    case 0x00000080:
                        prefix = "UPPER ";
                        break;
                           
                    case 0x00000140:
                        prefix = "LOWER ";
                        break;
                                
                }

                section.VstParameterName = prefix + section.VstParameterName;
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
                    default:
                        break;
                }
            }
            
           
            
        }
    }
}