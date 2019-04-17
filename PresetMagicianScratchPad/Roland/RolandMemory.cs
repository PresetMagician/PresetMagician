using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GSF;

namespace PresetMagicianScratchPad
{
    public class RolandMemory
    {
        private Dictionary<int, byte> Memory = new Dictionary<int, byte>();
        private List<int> UnreadMemory = new List<int>();
        private byte[] FileData;

        public void ReadFile(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            FileData = File.ReadAllBytes(file);
        }

        public void SetFileData(byte[] data)
        {
            FileData = data;
        }

        public byte[] GetFromFile(int address, int length)
        {
            
            if (FileData == null)
            {
                return null;
            }
            if (address +length> FileData.Length)
            {
                return null;
            }
            
            var b = new byte[length];
            for (var i = 0; i < length; i++)
            {
                b[i] = FileData[address + i];
            }

            return b;
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
        public byte Get(int address)
        {
            if (Memory.ContainsKey(address))
            {
                UnreadMemory.Remove(address);
                return Memory[address];
            }

            return 0;
        }
        
        public int Get(int address, int bytes)
        {
            var byteVal = new byte[4];

            for (var i = 0; i < bytes; i++)
            {
                if (i > 3)
                {
                    if (Get(address + i) != 0)
                    {
                        throw new Exception("FOO");
                    }
                }
                else
                {
                    byteVal[i] = Get(address + i);
                }
            }

            if (address == 0x00600128 && bytes == 8 && byteVal[0] != 0)
            {
                Debug.WriteLine("FOO");
            }

            return LittleEndian.ToInt32(byteVal,0);
        }

        public void Load(string file)
        {
            Memory.Clear();
            var data = File.ReadAllBytes(file);

            using (var ms = new MemoryStream(data))
            {
                while (ms.Position != ms.Length)
                {
                    var buffer = new byte[4];

                    ms.Read(buffer, 0, 4);


                    var address = BigEndian.ToInt32(buffer, 0);

                    ms.Read(buffer, 0, 4);

                    var writeZeroes = false;
                    for (int i = 3; i > -1; i--)
                    {
                        var val =  buffer[3 -i];
                        if (val != 0 || i == 0 ||writeZeroes)
                        {
                            Set(address+i, val);
                        }
                    }
                }
            }
        }

        public void Dump(string outputFile)
        {
            var size = Memory.Keys.Max()+1;
            
            var buffer = new byte[size];

            foreach (var address in Memory.Keys)
            {
                buffer[address] = Memory[address];
               
            }
            
            File.WriteAllBytes(outputFile, buffer);
        }

        public List<int> GetUnreadMemory()
        {
            return UnreadMemory.ToList();
        }

        public bool Has(int address)
        {
            return Memory.ContainsKey(address);
        }
        
        public bool Has(int address, int bytes)
        {
            for (var i = 0; i < bytes; i++)
            {
                if (Memory.ContainsKey(address+i))
                {
                    return true;
                }
            }

            return false;
        }

        public void Set(int address, byte value)
        {
            if (!Memory.ContainsKey(address))
            {
                Memory.Add(address, value);
                UnreadMemory.Add(address);
            }
            else
            {
                if (Memory[address] != 0 && Memory[address] != value && address < 0x10000000)
                {
                    Debug.WriteLine(
                        $"Warning trying to overwrite 0x{address:x8} with {value}, existing value {Memory[address]}");
                }

                Memory[address] = value;
            }
        }
    }
}