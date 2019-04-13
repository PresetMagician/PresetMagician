using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandValueArray: RolandValue
    {
        private int _tempSize;
        public RolandValueArray(RolandMemorySection parent, XElement node) : base(parent, node)
        {
        }

        private readonly List<int> _addresses = new List<int>();

        public override void Parse()
        {
            base.Parse();

            if (_addresses.Count == 0)
            {
                
            }
            else
            {
                Offset = _addresses.Min();
            }
        }
        
        protected override int ApplySize(int size)
        {
            _tempSize = size;
            return GetTypeSize() * size;
        }
        
        protected override void ApplyAddress(int newAddress)
        {
            if (newAddress == -1)
            {
                _tempSize += 1;
            }
            HasOffset = true;
            var newAddressEnd = newAddress + GetTypeSize();
            foreach (var existingAddress in _addresses)
            {
                var existingAddressEnd = existingAddress + GetTypeSize();
                
                if ((existingAddressEnd > newAddress && existingAddressEnd <= newAddressEnd) ||
                    existingAddress > newAddress && existingAddress < newAddressEnd)
                {
                    Debug.WriteLine($"RolandValueArray.ApplyAddress: This address overlaps with an existing address at line {((IXmlLineInfo) SourceNode).LineNumber}");
                }
            }
            _addresses.Add(newAddress);
        }
        
        
        
        protected override void CalculateSize()
        {
            if (_addresses.Count == 0)
            {
                Size = _tempSize;
                return;
            }
            var min = _addresses.Min();
            var max = _addresses.Max();
            
            var length = (max - min) + GetTypeSize();

            if (_tempSize != 0)
            {
                Size = _tempSize;
            }
            else
            {
               

                Size = length;
            }

           
        }
        
        public override string GetTypeName()
        {
            return $"ValueArray(PTL{GetPrimitiveTypeLength()})";
        }
        
        public override string GetDumpData(RolandMemory memory = null)
        {
            if (memory == null)
            {
                return null;
            }

            var hexValues = new List<string>();


            string hexValuesString;
            switch (_primitiveTypeName)
            {
                case "int":
                    var numberValues = new List<string>();
                    
                    foreach (var address in _addresses)
                    {
                        var memoryValue = memory.Get(StartAddress + address, GetPrimitiveTypeLength());
                        var numBits = _primitiveTypeBits * (int) _primitiveTypeLength;
                
                        var intValue = GetValue(memoryValue, numBits, 0);

                        var strVal = intValue.ToString("x" + GetPrimitiveTypeLength());
                        hexValues.Add($"0x{strVal}");
                        numberValues.Add(intValue.ToString());
                        
                        
                    }

                    hexValuesString = string.Join(" ", hexValues);
                    var numberValuesString = string.Join(", ", numberValues);

                    return $"{hexValuesString} ({numberValuesString})";
                case "string":
                    var finalString = "";
                    
                    if (Type == "stringNx7")
                    {
                        for (var i = 0; i < Size; i++)
                        {
                            var charValue2 = (char)memory.Get(StartAddress + i, 1);
                            if (charValue2 != 0)
                            {
                                finalString += charValue2;
                                hexValues.Add($"0x{(int) charValue2:x2}");
                            }
                        }
                    }
                    else
                    {
                        

                        for (var i = 0; i < Size; i+=4)
                        {
                            memory.Get(StartAddress + i + 2, 2);
                            var memoryValue = memory.Get(StartAddress + i, GetPrimitiveTypeLength());
                            var numBits = _primitiveTypeBits * (int) _primitiveTypeLength;

                            var charValue = (char) GetValue(memoryValue, numBits, 1);
                            if (charValue != 0)
                            {
                                finalString += charValue;
                            }

                            hexValues.Add($"0x{(int) charValue:x2}");

                            charValue = (char) GetValue(memoryValue, numBits, 0);
                            if (charValue != 0)
                            {
                                finalString += charValue;
                            }

                            hexValues.Add($"0x{(int) charValue:x2}");
                        }

                    }
                    
                    hexValuesString = string.Join(" ", hexValues);
                    return $"{hexValuesString} ({finalString})";

                default:
                        return "<not implemented";
                    
            }
           

            if (_primitiveTypeLength == null)
            {
                throw new Exception($"RolandValue.GetDumpData: Don't know how to handle {Type} with a non-set primitive length");
            }

            if (_primitiveTypeName == "int")
            {
                var intValue = GetValue(memory.Get(StartAddress), _primitiveTypeBits * (int)_primitiveTypeLength, 0);
                return $"0x{intValue:X} ({intValue})"; 
            }
            
            throw new Exception($"RolandValue.GetDumpData: Don't know how to handle the primitive {_primitiveTypeName} for {Type}");
            
            
        }
    }
}