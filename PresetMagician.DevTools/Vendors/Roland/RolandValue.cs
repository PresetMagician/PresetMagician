using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using PresetMagician.DevTools.Vendors.Roland;
using PresetMagician.VendorPresetParser.Roland.Internal;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandValue : RolandMemorySection
    {
        public RolandValue(RolandMemorySection parent, XElement node)
        {
            Parent = parent;
            SourceNode = node;
        }

        public string DefaultValue { get; set; }
        public string Range { get; set; }


        public int _primitiveTypeBits;
        public int _primitiveTypeLength;
        public string _primitiveTypeName;


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

        public RolandValueStruct ToRolandValueStruct()
        {
            var rvs = new RolandValueStruct
            {
                ValuePath = ValuePath,
                FileAddress = FileAddress,
                FileSize = FileSize,

                MemoryAddress = StartAddress,
                MemorySize = Size,

                PrimitiveTypeName = _primitiveTypeName,
                PrimitiveTypeBits = _primitiveTypeBits,
                PrimitiveTypeLength = _primitiveTypeLength,

                Range = Range,
                DefaultValue = DefaultValue
            };

            return rvs;
        }


        public override void DumpToPatchFile(RolandMemory memory, MemoryStream ms, RolandExportConfig exportConfig)
        {
            /*if (!exportConfig.ExportZeroForInt1X7 && Type == "int1x7" && memVal == 0)
            {

                return;
            }*/


            if (!exportConfig.ShouldExport(ValuePath))
            {
                return;
            }

            var rvs = ToRolandValueStruct();
            var result = rvs.ConvertMemoryValue(memory);

            if (result.address != null && result.data != null)
            {
                ms.Write(result.address, 0, 4);
                ms.Write(result.data, 0, 4);
            }
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
                _primitiveTypeLength = (int) result.length;
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
                memVal = GetValue(val, _primitiveTypeBits * (int) _primitiveTypeLength, 0);
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

            var fileVal = RolandValueStruct.DecodeValueAsInt(data, _primitiveTypeLength, _primitiveTypeBits);
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
                    memVal = GetValue(val, _primitiveTypeBits * (int) _primitiveTypeLength, 0);

                    valueStr = $"MemVal: 0x{memVal:X} ({memVal})";
                }


                var data = memory.GetFromFile(FileAddress, _primitiveTypeLength);


                if (data != null)
                {
                    fileVal = RolandValueStruct.DecodeValueAsInt(data, _primitiveTypeLength, _primitiveTypeBits);
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

            throw new Exception(
                $"RolandValue.GetDumpData: Don't know how to handle the primitive {_primitiveTypeName} for {Type}");
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
            return memory.Has(StartAddress, GetPrimitiveTypeLength());
        }

        public int GetValue(RolandMemory memory)
        {
            var val = memory.Get(StartAddress, GetPrimitiveTypeLength());
            var intValue = GetValue(val, _primitiveTypeBits * (int) _primitiveTypeLength, 0);

            return intValue;
        }

        public static int GetValue(int memoryData, int numberBase, int position)
        {
            return memoryData;
        }
    }
}