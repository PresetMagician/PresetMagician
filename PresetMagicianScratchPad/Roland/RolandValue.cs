using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using GSF;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandValue: RolandMemorySection
    {
        public RolandValue(RolandMemorySection parent, XElement node)
        {
            Parent = parent;
            SourceNode = node;
            ExportNode = new XElement("value");
        }
        
        public string DefaultValue { get; set; }
        public string Range { get; set; }
        

        private bool _hasAddress;
        protected int _primitiveTypeBits;
        protected int _primitiveTypeLength;
        protected string _primitiveTypeName;
        
       
        
        public override void ApplyProperties()
        {
            ApplyType();
            
            foreach (var childElement in SourceNode.Elements())
            {
                var elementValue = childElement.Value;

                switch (childElement.Name.ToString())
                {
                    case "name":
                        Name = elementValue;
                        VstParameterName = elementValue;
                        ParentValuePath.AddRange(Parent.ParentValuePath);
                        ParentValuePath.Add(Name);
                        break;
                    case "address":
                        if (!HasOffset)
                        {
                            //Offset = ParseAddress(elementValue, Parent.StartAddress);
                            //StartAddress = Parent.StartAddress + Offset;
                        }

                        break;
                    case "size":
                        IsAutoCalculatedSize = false;
                        Size = ParseSize(elementValue);
                        break;
                    case "default":
                        DefaultValue = elementValue;
                        break;
                    case "range":
                        Range = elementValue;
                        break;
                    case "type":
                        break;
                    /*case "size":
                        Size = ApplySize(ParseSize(elementValue));
                        break;*/
                    default:
                        Debug.WriteLine(
                            $"RolandValue.ApplyProperties: Unknown element {childElement.Name} at line {((IXmlLineInfo) childElement).LineNumber}");
                        break;
                }
            }
            
            ValuePath = string.Join(".", ParentValuePath);

            if (IsAutoCalculatedSize)
            {
                CalculateSize();
            }
            
            CalculatedSize = GetTypeSize();
            
            FileSize = Size;
            
            DoCallback("RolandValue.AfterParse");
        }

        protected virtual int ApplySize(int size)
        {
            IsAutoCalculatedSize = false;
            return GetTypeSize() * size;
        }

        public bool IsValueInsideRange(int value)
        {
            if (string.IsNullOrWhiteSpace(Range))
            {
                return true;
            }

            var ranges = Range.Split(',');

            if (ranges.Length != 2)
            {
                throw new Exception($"Expected range to be specified in 2 substrings, but got {ranges.Length}");
            }

            var range1 = ranges[0];
            var range2 = ranges[1];

            if (!int.TryParse(range1, out int range1int))
            {
                throw new Exception($"Could not int-parse range1 {range1}");
            }
            
            if (!int.TryParse(range2, out int range2int))
            {
                throw new Exception($"Could not int-parse range2 {range2}");
            }

            if (value >= range1int && value <= range2int)
            {
                return true;
            }

            return false;
        }

        public void DumpToPatchFile(RolandMemory memory, MemoryStream ms, RolandExportConfig exportConfig)
        {
            if (_primitiveTypeName == "string")
            {
                return;
            }

          

            int? defaultValue = null;
            
            if (int.TryParse(DefaultValue, out int defaultInt))
            {
                defaultValue = defaultInt;
            }
            
            
            
            var data = memory.GetFromFile(FileAddress, _primitiveTypeLength);

                
            if (data == null)
            {
                return;
            }
            
            
            

            var addr = BigEndian.GetBytes(StartAddress);

            var memVal = RolandMemory.DecodeValueAsInt(data, _primitiveTypeLength, _primitiveTypeBits);

            if (!exportConfig.ExportZeroForInt1X7 && Type == "int1x7" && memVal == 0)
            {

                return;
            }
            
            if (!IsValueInsideRange(memVal))
            {
                if (defaultValue != null)
                {
                    memVal = (int) defaultValue;
                }
                else
                {
                    throw new Exception("Value outside of range but default is not specified");
                }
            }

            bool forceInclude = exportConfig.IncludeExportValuePaths.Contains(ValuePath);

            foreach (var reg in exportConfig.IncludeExportValuePathsRegEx)
            {
                var r = exportConfig.GetRegex(reg);
                if (r.Match(ValuePath).Success)
                {
                    forceInclude = true;
                    break;
                }    
            }

            if (!forceInclude)
            {
                if (exportConfig.SkipExportValuePaths.Contains(ValuePath))
                {
                    return;
                }

                foreach (var reg in exportConfig.SkipExportValuePathsRegEx)
                {
                    var r = exportConfig.GetRegex(reg);
                    if (r.Match(ValuePath).Success)
                    {
                        return;
                    }
                }
            }



           
           
            var value = BigEndian.GetBytes(memVal);

            

            ms.Write(addr,0,4);
            ms.Write(value,0,4);
        }

        protected virtual void CalculateSize()
        {
            Size = GetTypeSize();
        }

      
        private void ApplyType()
        {
            var typeElement = SourceNode.Element("type");

            if (typeElement == null)
            {
                Type = "int1x7";
            }
            else
            {

                Type = typeElement.Value;
            }

            var result = ParseTypeString(Type);

            _primitiveTypeBits = result.bits;
            
            _primitiveTypeName = result.primitiveType;

            if (result.length == null)
            {
                // Type-specific manual overrides
                switch (Type)
                {
                    case "stringNx4":
                        _primitiveTypeLength = 2;
                        break;
                    case "stringNx7":
                        _primitiveTypeLength = 2;
                        break;
                    default:
                        throw new Exception("Unknown manual override");
                }
            }
            else
            {
                _primitiveTypeLength = (int)result.length;
            }


        }

        public static (int? length, int bits, string primitiveType) ParseTypeString(string typeString)
        {
            var separatorPos = typeString.IndexOf("x", StringComparison.InvariantCulture);

            if (separatorPos == -1)
            {
                throw new Exception($"Unable to parse type string {typeString}: No separator found");
            }

            var bits = int.Parse(typeString.Substring(separatorPos + 1));
            var type = typeString.Substring(0, separatorPos - 1);
            var lengthStr = typeString.Substring(separatorPos - 1, 1);
            
            switch (type)
            {
                case "int":
                case "string":
                    break;
                default:
                    throw new Exception($"Unknown type '{type}'");
                
            }
            if (lengthStr == "N")
            {
                return (null, bits, type);
            }

            return (int.Parse(lengthStr), bits, type);

        }

       

        /// <summary>
        /// Returns the size of this type in bytes
        /// </summary>
        /// <returns></returns>
        protected int GetTypeSize()
        {
            return GetPrimitiveTypeLength();
           
        }

        public (bool, string) CompareAgainstFile(RolandMemory memory)
        {
            if (_primitiveTypeName == "string")
            {
                return (true, "");
            }
            var memVal = 0;
            var retString = $"{Name} ";
            if (memory.Has(StartAddress, GetPrimitiveTypeLength()))
            {
                var val = memory.Get(StartAddress, GetPrimitiveTypeLength());
                memVal = GetValue(val, _primitiveTypeBits * (int)_primitiveTypeLength, 0);
                retString += $"MemVal: 0x{memVal:x2} ";

            }
            else
            {
                if (int.TryParse(DefaultValue, out int defaultInt))
                {
                    memVal = defaultInt;
                    retString += $"DefaultVal: 0x{memVal:x2} ";
                }
                else
                {
                    retString += "NoDefaultVal/NoMemoryVal";
                    return (false, retString);                    
                }

                
            }
            
            var data = memory.GetFromFile(FileAddress, _primitiveTypeLength);


            if (data == null)
            {
                return (false, "file value does not exist");
            }
            
            var fileVal = RolandMemory.DecodeValueAsInt(data, _primitiveTypeLength, _primitiveTypeBits);
            if (fileVal == memVal)
            {
                return (true, "");
            }
            else
            {
                retString += $" fileVal 0x{fileVal:x2}";
                return (false, retString);
            }
            
        }
        
        
      
        public override string GetDumpData(RolandMemory memory = null)
        {
            if (memory == null)
            {
                return null;
            }

            if (_primitiveTypeLength == null)
            {
                throw new Exception($"RolandValue.GetDumpData: Don't know how to handle {Type} with a non-set primitive length");
            }
            
            var fileValueStr = "";
            var valueStr = "";
            var defaultStr = "";
            var equalStr = "";

            if (_primitiveTypeName == "int")
            {
                int? memVal = null;
                int? fileVal = null;
                int? defVal = null;
                
                if (memory.Has(StartAddress, GetPrimitiveTypeLength()))
                {
                    var val = memory.Get(StartAddress, GetPrimitiveTypeLength());
                    memVal = GetValue(val, _primitiveTypeBits * (int)_primitiveTypeLength, 0);
                    
                    valueStr = $"MemVal: 0x{memVal:X} ({memVal})";
                }
               
                

                var data = memory.GetFromFile(FileAddress, _primitiveTypeLength);

                
                if (data != null)
                {
                    fileVal = RolandMemory.DecodeValueAsInt(data, _primitiveTypeLength, _primitiveTypeBits);
                    fileValueStr = $"FileVal: 0x{fileVal:X} ({fileVal})";

                }

                if (int.TryParse(DefaultValue, out int defaultInt))
                {
                    defVal = defaultInt;
                    defaultStr = $"DefVal: 0x{defaultInt:X} ({defaultInt})";
                }

                if (fileVal != null)
                {
                    if (memVal != null)
                    {
                        if (fileVal == memVal)
                        {
                            equalStr = "EQ";
                        }
                    }
                    else
                    {
                        if (fileVal == 0)
                        {
                            equalStr = "EQ?";
                        }
                        if (defVal != null)
                        {
                            if (fileVal == defVal)
                            {
                                equalStr = "EQ";
                            }
                        }
                       
                        
                        else
                        {
                            equalStr = "NODEFAULT";
                        }
                    }
                }
                
                return $"{valueStr.PadRight(25)} {fileValueStr.PadRight(25)} {defaultStr.PadRight(25)} {equalStr}"; 
            }

            return "";
            
            throw new Exception($"RolandValue.GetDumpData: Don't know how to handle the primitive {_primitiveTypeName} for {Type}");
            
            
        }
        
        

        public override string GetTypeName()
        {
            return $"Value(PTL{GetPrimitiveTypeLength()})";
        }

        /// <summary>
        /// Returns the size of this type in bytes
        /// </summary>
        /// <returns></returns>
        public int GetPrimitiveTypeLength()
        {
            return _primitiveTypeLength;
        }

        public bool HasValue(RolandMemory memory)
        {
            return  memory.Has(StartAddress, GetPrimitiveTypeLength());
        }
        public int GetValue(RolandMemory memory)
        {
            var val = memory.Get(StartAddress, GetPrimitiveTypeLength());
            var intValue = GetValue(val, _primitiveTypeBits * (int)_primitiveTypeLength, 0);

            return intValue;
        }

        public static int GetValue(int memoryData, int numberBase, int position)
        {
            /*var orig = LittleEndian.GetBytes(memoryData);

            var startBit = position * numberBase;
            var endBit = (position + 1) * numberBase - 1;
            var b = new byte[4];*/
            
            
            return memoryData;
            var shiftedMemoryData = memoryData >> (numberBase * position);
            var mask = (int)Math.Pow(2, numberBase) - 1;
            return shiftedMemoryData & mask;

        }
    }
}