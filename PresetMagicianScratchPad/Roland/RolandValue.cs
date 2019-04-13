using System;
using System.ComponentModel;
using System.Diagnostics;
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
                        break;
                    case "address":
                        ApplyAddress(ParseAddress(elementValue));
                        break;
                    case "default":
                        DefaultValue = elementValue;
                        break;
                    case "range":
                        Range = elementValue;
                        break;
                    case "type":
                        break;
                    case "size":
                        Size = ApplySize(ParseSize(elementValue));
                        break;
                    default:
                        Debug.WriteLine(
                            $"RolandValue.ApplyProperties: Unknown element {childElement.Name} at line {((IXmlLineInfo) childElement).LineNumber}");
                        break;
                }
            }

            if (IsAutoCalculatedSize)
            {
                CalculateSize();
            }
            
            FileSize = Size;
            
            DoCallback("RolandValue.AfterParse");
        }

        protected virtual int ApplySize(int size)
        {
            IsAutoCalculatedSize = false;
            return GetTypeSize() * size;
        }

        protected virtual void CalculateSize()
        {
            Size = GetTypeSize();
        }

        protected virtual void ApplyAddress(int address)
        {
            if (address == -1)
            {
                return;
            }
            if (_hasAddress)
            {
                throw new Exception($"RolandValue.ApplyAddress: Multiple addresses found for a single value  at line {((IXmlLineInfo) SourceNode).LineNumber}");
            }

            Offset = address;
            HasOffset = true;
            _hasAddress = true;
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
            switch (Type)
            {
                case "int1x7":
                    return 1;
                case "int1x1":
                    return  1;
                case "int4x4":
                    return 2;
                case "int2x4":
                    return 2;
                case "int8x4":
                    return 8;
                case "int3x4":
                    return  3;
                case "stringNx4": 
                    return 1;
                case "stringNx7":
                    return 1;
                default:
                    throw new Exception($"RolandValue.GetTypeSize: No size found for type {Type}");
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