using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using PresetMagician.Utils;

namespace PresetMagicianScratchPad.Roland
{
    public abstract class RolandMemorySection
    {
        public static HashSet<string> RegisteredCallbacks = new HashSet<string>
        {
            "AfterApplyProperties", "RolandStructBeforeApplyChilds",
            "RolandValue.AfterParse"
        };

        /// <summary>
        /// The source node from where this memory section was defined
        /// </summary>
        public XElement SourceNode { get; protected set; }

        public RolandMemorySection Parent { get; protected set; }

        /// <summary>
        /// The absolute start address in bytes
        /// </summary>
        public int StartAddress { get; set; }
        
        public int FileAddress { get; set; }
        public int FileSize { get; set; }

        /// <summary>
        /// The length in bytes
        /// </summary>
        public int Size { get; set; }

        public bool IsAutoCalculatedSize { get; protected set; } = true;


        public int Offset { get; set; }

        public bool HasOffset { get; set; }

        public int EndAddress
        {
            get { return StartAddress + Size; }
        }


        public string Type { get; set; }
        public string Name { get; set; }

        public List<RolandStruct> Structs { get; } = new List<RolandStruct>();
        public List<RolandValue> Values { get; } = new List<RolandValue>();


        protected void GetValueByNameInt(string name, List<RolandValue> values)
        {
            foreach (var value in Values)
            {
                if (value.Name == name)
                {
                    values.Add(value);
                }
            }

            foreach (var subStruct in Structs)
            {
                subStruct.GetValueByNameInt(name, values);
              
            }
        }
        public List<RolandValue> GetValueByName(string name)
        {
            var list = new List<RolandValue>();

            GetValueByNameInt(name, list);

            return list;
          
        }


        /// <summary>
        /// Parses the XElement node of this section
        /// </summary>
        public virtual void Parse()
        {
            ApplyProperties();
            DoCallback("AfterApplyProperties");
        }

        public delegate void ParseCallback(RolandMemorySection section);

        protected void DoCallback(string type)
        {
            if (!RegisteredCallbacks.Contains(type))
            {
                throw new Exception($"Callback {type} does not exist");
            }

            if (Callbacks.ContainsKey(type))
            {
                Callbacks[type].Invoke(this);
            }
        }

        public void RegisterCallback(string type, ParseCallback callback)
        {
            if (!RegisteredCallbacks.Contains(type))
            {
                throw new Exception($"Callback {type} does not exist");
            }

            Callbacks.Add(type, callback);
        }

        public Dictionary<string, ParseCallback> Callbacks { get; set; } = new Dictionary<string, ParseCallback>();

        /// <summary>
        /// Applies the properties of the XElement node
        /// </summary>
        public abstract void ApplyProperties();

        public string Dump(RolandMemory memory = null)
        {
            return DumpInternal(0, memory);
        }

        public (RolandMemorySection closestSection, int distance) FindClosestToAddress(int address)
        {
            var closestSection = this;
            var distance = Math.Abs(address - closestSection.StartAddress);
            foreach (var subStruct in Structs)
            {
                var result = subStruct.FindClosestToAddress(address);

                if (result.distance < distance)
                {
                    closestSection = result.closestSection;
                    distance = result.distance;
                }
            }
            
            foreach (var value in Values)
            {
                var result = value.FindClosestToAddress(address);

                if (result.distance < distance)
                {
                    closestSection = result.closestSection;
                    distance = result.distance;
                }
            }

            return (closestSection, distance);
        }

        public string GetInfo(RolandMemory memory = null)
        {
            return GetInfo(0, memory);
        }
        
        public string GetInfo(int indent, RolandMemory memory = null)
        {
            string pad = "";
            for (var i = 0; i < indent; i++)
            {
                pad = pad + "  ";
            }

            var desc = $"{GetTypeName()} {Name} ({Type})";


            var data = GetDumpData(memory);
            var dataStr = "";

            if (data != null)
            {
                dataStr = $"Data: {data}";
            }

            var sizeFlag = "[O]";
            if (IsAutoCalculatedSize)
            {
                sizeFlag = "[A]";
            }
            
            return $"{pad}{desc}".PadRight(60) +
                          $"0x{StringUtils.Int32ToHexString(StartAddress)} - 0x{StringUtils.Int32ToHexString(EndAddress)} (Len {sizeFlag} 0x{Size:x8} FileAddr: 0x{FileAddress:x8} FileLen: 0x{FileSize:x4} {dataStr}";

        }
        private string DumpInternal(int indent, RolandMemory memory = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(GetInfo(indent, memory));
           
            var dumpStuff = new Dictionary<int, List<string>>();
            foreach (var subStruct in Structs)
            {
                if (!dumpStuff.ContainsKey(subStruct.StartAddress))
                {
                    dumpStuff.Add(subStruct.StartAddress, new List<string>());
                }

                var dumpStr = subStruct.DumpInternal(indent + 1, memory);
                dumpStuff[subStruct.StartAddress].Add(dumpStr);
            }

            foreach (var value in Values)
            {
                if (!dumpStuff.ContainsKey(value.StartAddress))
                {
                    dumpStuff.Add(value.StartAddress, new List<string>());
                }

                var dumpStr = value.DumpInternal(indent + 1, memory);
                dumpStuff[value.StartAddress].Add(dumpStr);
            }

            var addresses = dumpStuff.Keys.ToList();
            addresses.Sort();

            foreach (var address in addresses)
            {
                foreach (var dumpStr in dumpStuff[address])
                {
                    sb.Append(dumpStr);
                }
            }

            return sb.ToString();
        }

        public virtual string GetTypeName()
        {
            return GetType().Name;
        }

        public virtual string GetDumpData(RolandMemory memory = null)
        {
            return null;
        }

        protected static XElement GetStructType(XDocument document, string typeName)
        {
            var xpath = $"/script/structType/type[text() = '{typeName}']";

            if (document.Document == null)
            {
                throw new Exception("The document of the given node is null");
            }

            var foundNode = document.Document.XPathSelectElement(xpath);

            if (foundNode == null)
            {
                throw new Exception($"Unable to find structType with type {typeName}");
            }

            return foundNode.Parent;
        }

        protected static int ParseAddress(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }

            var hexValuesSplit = value.Trim().Split(' ').ToList();


            if (hexValuesSplit.Count == 1)
            {
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 2)
            {
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 3)
            {
                hexValuesSplit = hexValuesSplit.Prepend("00").ToList();
            }

            if (hexValuesSplit.Count != 4)
            {
                Debug.WriteLine($"Warning: {value} is not 4 bytes.");
            }

            var ints = new List<int>();

            foreach (var hexValueString in hexValuesSplit)
            {
                ints.Add(Convert.ToByte(hexValueString, 16));
            }

            return ints[0] * 128 * 128 * 128 + ints[1] * 128 * 128 + ints[2] * 128 + ints[3];
        }

        protected static int ParseSize(string value)
        {
            return ParseAddress(value);
        }
    }
}