using System;
using GSF;

namespace PresetMagician.VendorPresetParser.Roland.Internal
{
    public class RolandValueStruct
    {
        public string ValuePath { get; set; }
        public int MemoryAddress { get; set; }
        public int MemorySize { get; set; }
        
        public int FileAddress { get; set; }
        public int FileSize { get; set; }
        
        public string PrimitiveTypeName { get; set; }
        public int PrimitiveTypeLength { get; set; }
        public int PrimitiveTypeBits { get; set; }
        
        public string Range { get; set; }
        public string DefaultValue { get; set; }

        private bool OverrideMemory { get; set; }
        private int OverrideMemoryValue { get; set; }

        public int? GetMemoryValue(RolandFileMemory memory)
        {
            var data = memory.GetFromFile(FileAddress, PrimitiveTypeLength);
                
            if (data == null)
            {
                return null;
            }
            
            return DecodeValueAsInt(data, PrimitiveTypeLength, PrimitiveTypeBits);
        }

        public void SetMemoryValue(int value)
        {
            OverrideMemory = true;
            OverrideMemoryValue = value;
        }
        
        public (byte[] address, byte[] data) ConvertMemoryValue(RolandFileMemory memory)
        {
            if (PrimitiveTypeName == "string")
            {
                return (null, null);
            }
            
            int? defaultValue = null;
            
            if (int.TryParse(DefaultValue, out int defaultInt))
            {
                defaultValue = defaultInt;
            }

            var tmpMemVal = GetMemoryValue(memory);

            if (tmpMemVal == null)
            {
                return (null, null);
            }
            
            var memVal = (int)tmpMemVal;

            if (OverrideMemory)
            {
                memVal = OverrideMemoryValue;
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
            
            var addr = BigEndian.GetBytes(MemoryAddress);
            
            return (addr, BigEndian.GetBytes(memVal));

        }
        
        public static int DecodeValueAsInt(byte[] data, int length, int bitSize)
        {
            uint tmp = 0;
            
            for (var i = 0; i < length; i++)
            {
                tmp = (tmp << bitSize) ^ data[i];
            }

            var bTmp = LittleEndian.GetBytes(tmp);
            return LittleEndian.ToInt32(bTmp,0);
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
    }
}