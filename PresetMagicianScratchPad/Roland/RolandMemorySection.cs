using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Catel.Collections;
using PresetMagician.Utils;
using PresetMagician.Utils.BinaryStructViz;

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

        public string VstParameterName { get; set; }

        public RolandMemorySection Parent { get; protected set; }

        public string ValuePath { get; set; } = "";

        public List<string> ParentValuePath = new List<string>();

        public int ArrayIndex { get; set; }
        public bool UseArrayIndex = false;

        /// <summary>
        /// The absolute start address in bytes
        /// </summary>
        public int StartAddress { get; set; }

        public int ChildOffset { get; set; }
        public int FileOffset { get; set; }


        public int FileAddress { get; set; }
        public int FileSize { get; set; }
        
        public RolandExportConfig Config { get; set; }

        /// <summary>
        /// The length in bytes
        /// </summary>
        public int Size { get; set; }

        public int CalculatedSize { get; set; }

        public bool IsAutoCalculatedSize { get; protected set; } = true;


        public int Offset { get; set; }

        public bool HasOffset { get; set; }

        public int EndAddress => StartAddress + Size;


        public string Type { get; set; }
        public string Name { get; set; }

        public List<RolandStruct> Structs { get; } = new List<RolandStruct>();
        public List<RolandValue> Values { get; } = new List<RolandValue>();

        public RolandMemorySection FindByDottedName(string name)
        {
            var names = name.Split('.').ToList();

            return FindByDottedNameInt(names);
        }

        protected RolandMemorySection FindByDottedNameInt(List<string> parts)
        {
            var firstPartName = parts.FirstOrDefault();

            if (firstPartName == null)
            {
                return this;
            }

            foreach (var subSection in Structs)
            {
                if (subSection.Name == firstPartName)
                {
                    parts.RemoveFirst();
                    return subSection.FindByDottedNameInt(parts);
                }
            }

            foreach (var value in Values)
            {
                if (value.Name == firstPartName)
                {
                    parts.RemoveFirst();
                    return value.FindByDottedNameInt(parts);
                }
            }

            return null;
        }

        protected void GetVstParameterValueByNameInt(string name, List<RolandValue> values)
        {
            foreach (var value in Values)
            {
                if (value.VstParameterName == name)
                {
                    values.Add(value);
                }
            }

            foreach (var subStruct in Structs)
            {
                subStruct.GetVstParameterValueByNameInt(name, values);
            }
        }

        public List<RolandValue> GetVstParameterValueByName(string name)
        {
            var list = new List<RolandValue>();

            GetVstParameterValueByNameInt(name, list);

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

        public List<FooVizDataWithStart> GetStructureData(List<FooVizDataWithStart> data = null)
        {
            if (data == null)
            {
                data = new List<FooVizDataWithStart>();
            }

            foreach (var subStruct in Structs)
            {
                var sizeFlag = "[O]";
                if (subStruct.IsAutoCalculatedSize)
                {
                    sizeFlag = "[A]";
                }
                else
                {
                    if (subStruct.CalculatedSize != subStruct.Size)
                    {
                        sizeFlag += "[MS]";
                    }
                }

                data.Add(new FooVizDataWithStart
                {
                    Start = subStruct.FileAddress, Length = subStruct.FileSize,
                    Content = subStruct.ValuePath + Environment.NewLine +
                              $"{sizeFlag} Len: 0x{subStruct.Size:X} FLen: 0x{subStruct.FileSize:X} FAddr: 0x{subStruct.FileAddress:X}"
                });
                subStruct.GetStructureData(data);
            }

            foreach (var value in Values)
            {
                var sizeFlag = "[O]";
                if (value.IsAutoCalculatedSize)
                {
                    sizeFlag = "[A]";
                }
                else
                {
                    if (value.CalculatedSize != value.Size)
                    {
                        sizeFlag += "[MV]";
                    }
                }

                data.Add(new FooVizDataWithStart
                {
                    Start = value.FileAddress, Length = value.FileSize, Content = value.ValuePath +
                    Environment.NewLine +
                    $"{sizeFlag} Len: 0x{value.Size:X} FLen: 0x{value.FileSize:X} FAddr: 0x{value.FileAddress:X}"
                });
            }

            return data;
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

        public void DumpToPatchFile(RolandMemory memory, MemoryStream ms, RolandExportConfig exportConfig)
        {
            foreach (var subStruct in Structs)
            {
                subStruct.DumpToPatchFile(memory, ms, exportConfig);
            }

            foreach (var value in Values)
            {
                value.DumpToPatchFile(memory, ms, exportConfig);
            }
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

        public List<string> CompareAgainstFileMemory(RolandMemory memory)
        {
            var errors = new List<string>();

            foreach (var subStruct in Structs)
            {
                errors.AddRange(subStruct.CompareAgainstFileMemory(memory));
            }

            foreach (var value in Values)
            {
                var result = value.CompareAgainstFile(memory);

                if (!result.Item1)
                {
                    errors.Add(result.Item2);
                }
            }

            return errors;
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

            return $"{pad}{desc}".PadRight(60) + $"{ValuePath.PadRight(40)}" +
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

        public static int ParseAddress(string value, int parentAddress)
        {
            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }

            var hexValuesSplit = value.Trim().Split(' ').ToList();
            int bitMask = 4;

            if (hexValuesSplit.Count == 1)
            {
                bitMask = 1;
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 2)
            {
                bitMask = 2;
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 3)
            {
                bitMask = 3;
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


            var baseParentAddress = parentAddress & ~(((uint) Math.Pow(2, bitMask * 7) - 1));

            var result = (int) baseParentAddress + ints[0] * 128 * 128 * 128 + ints[1] * 128 * 128 + ints[2] * 128 +
                         ints[3];

            //Debug.WriteLine($"Parent address is {parentAddress:X} address is {value} bitMask is {bitMask} base is {baseParentAddress:X} result is {result:X}");
            return result;
        }

        public static bool IsAbsoluteAddress(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var hexValuesSplit = value.Trim().Split(' ').ToList();


            if (hexValuesSplit.Count == 4)
            {
                return true;
            }

            return false;
        }

        public static int CombineAddress(string value, int parentAddress)
        {
            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }

            var hexValuesSplit = value.Trim().Split(' ').ToList();
            int bitMask = 4;

            if (hexValuesSplit.Count == 1)
            {
                bitMask = 1;
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 2)
            {
                bitMask = 2;
                hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").ToList();
            }

            if (hexValuesSplit.Count == 3)
            {
                bitMask = 3;
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


            var baseParentAddress = parentAddress & ~(((uint) Math.Pow(2, bitMask * 7) - 1));

            var result = (int) baseParentAddress + ints[0] * 128 * 128 * 128 + ints[1] * 128 * 128 + ints[2] * 128 +
                         ints[3];

            //Debug.WriteLine($"Parent address is {parentAddress:X} address is {value} bitMask is {bitMask} base is {baseParentAddress:X} result is {result:X}");
            return result;
        }

        protected static int ParseSize(string value)
        {
            return ParseAddress(value, 0);
        }
    }
}