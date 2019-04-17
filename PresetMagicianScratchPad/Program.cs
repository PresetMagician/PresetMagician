using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Catel.IoC;
using CsvHelper;
using GSF;
using Newtonsoft.Json;
using PresetMagician;
using PresetMagician.Core.Extensions;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.TestVstHost;
using PresetMagician.Utils;
using PresetMagician.Utils.BinaryStructViz;
using PresetMagician.Utils.Logger;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using PresetMagician.VstHost.VST;
using PresetMagicianScratchPad.Roland;
using PresetMagicianScratchPad.Stuff;

namespace PresetMagicianScratchPad
{
  
    public class Program
    {
        static Dictionary<string, RolandStructTypeOld> _structTypes = new Dictionary<string, RolandStructTypeOld>();
        private static HashSet<string> valueTypes = new HashSet<string>();

        [STAThread]
        static void Main(string[] args)
        {
           /* var b = new byte[] {0x46, 0x61};

            Debug.WriteLine(RolandMemory.DecodeValueAsInt(b, 2, 7));
            return;*/
            var culture = CultureInfo.GetCultureInfo("en-US");

            //Culture for any thread
            CultureInfo.DefaultThreadCurrentCulture = culture;

            //Culture for UI in any thread
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            Debug.WriteLine(RolandMemorySection.ParseAddress("00 00 00 00", 0x0000008c));
            /*RolandMemorySection.ParseAddress("00 00", 0xFFFFFFF);
            RolandMemorySection.ParseAddress("00 00 00", 0xFFFFFFF);
            RolandMemorySection.ParseAddress("00 00 00 00", 0xFFFFFFF);*/
            //return;
            foreach (var configFile in Directory.EnumerateFiles(
                @"C:\Users\Drachenkatze\Documents\PresetMagician\TestVstHost\Patches", "ExportConfig.json",
                SearchOption.AllDirectories))
            {
                var exportConfig = JsonConvert.DeserializeObject<RolandExportConfig>(File.ReadAllText(configFile));
                
                var serviceLocator = ServiceLocator.Default;

                FrontendInitializer.RegisterTypes(serviceLocator);
                FrontendInitializer.Initialize(serviceLocator);

                var remoteVstService = serviceLocator.ResolveType<RemoteVstService>();

                var plugin = new Plugin {PluginLocation = new PluginLocation {DllPath = exportConfig.Plugin}};

                var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

                pluginInstance.LoadPlugin().Wait();

                var koaParser = new KoaBankFileParser(exportConfig.KoaFileHeader, exportConfig.PresetNameLength, exportConfig.PresetLength);

                var rolandScript = ParseScript(exportConfig.ScriptFile, exportConfig.CallbackHook);
                var fooViz = new FooViz();
                rolandScript.ExportNode.Save(exportConfig.ScriptFile+".modified");
                
                foreach (var patchDirectory in exportConfig.PresetDirectories)
                {
                    if (patchDirectory.Contains("D-50"))
                    {
                        //continue;
                    }
                    Debug.WriteLine($"Processing patch directory {patchDirectory}");
                    foreach (var patchFile in Directory.EnumerateFiles(
                        patchDirectory, "*.bin",
                        SearchOption.AllDirectories))
                    {
                        var presets = koaParser.Parse(patchFile);
                        Debug.WriteLine($"Processed patch file {patchFile}, found {presets.Count} patches");

                        foreach (var preset in presets)
                        {
                            var currentPreset =
                                $"{Path.GetFileName(preset.BankFile)}.{preset.Index}.{PathUtils.SanitizeFilename(preset.PresetName.Trim())}";
                            var outputFile = Path.Combine(exportConfig.OutputDirectory,
                                currentPreset);
                            
                            var originalPatchDump = outputFile + ".originalpatchdump";
                            
                            var okPluginPatchDumpFile = outputFile + ".okpluginpatchdump";
                            
                            var memory = new RolandMemory();

                            
                            memory.SetFileData(preset.PresetData);
                            fooViz.SetMemory(preset.PresetData);

                            var structs = rolandScript.GetStructureData();

                            var sortedStructs = (from str in structs orderby str.Length descending , str.Start select str);

                            foreach (var sortedStruct in sortedStructs)
                            {
                                fooViz.AddStructure(sortedStruct.Start, sortedStruct.Length, sortedStruct.Content);
                            }
                            fooViz.DumpToFile(outputFile+".xlsx");

                            using (var ms = new MemoryStream())
                            {
                                
                                
                                Debug.WriteLine($"Processed {currentPreset}");
                                
                                var sw = new Stopwatch();
                                sw.Start();
                                rolandScript.DumpToPatchFile(memory, ms, exportConfig);

                                var suffixData = File.ReadAllBytes(exportConfig.SuffixFile);
                                ms.Write(suffixData, 0, suffixData.Length);
                                
                                

                                
                                Directory.CreateDirectory(exportConfig.OutputDirectory);
                                try
                                {
                                    var sortedDump = SortDump(ms.ToArray());

                                    byte[] sortedPluginDump;
                                    /*if (File.Exists(okPluginPatchDumpFile))
                                    {
                                        sortedPluginDump = SortDump(File.ReadAllBytes(okPluginPatchDumpFile));
                                    }
                                    else
                                    {*/
                                        pluginInstance.SetChunk(sortedDump, false);

                                        sortedPluginDump = SortDump(pluginInstance.GetChunk(false));
                                    //}

                                    var sortedDumpEqual = sortedDump.SequenceEqual(sortedPluginDump);
                                    var sortedOriginalDumpEqual = true;
                                    if (File.Exists(originalPatchDump))
                                    {
                                        var sortedOriginal = SortDump(File.ReadAllBytes(originalPatchDump));
                                        sortedOriginalDumpEqual = sortedDump.SequenceEqual(sortedOriginal);
                                        Debug.WriteLine("OrigPatchDump");

                                    }
                                    
                                    var patchDumpFile = outputFile + ".patchdump";
                                    var structureDumpFile  = outputFile + ".structure";
                                    var pluginPatchDumpFile = outputFile + ".pluginpatchdump";
                                    
                                        
                                    var presetDataFile = outputFile + ".presetdata";
                                    File.WriteAllBytes( patchDumpFile, sortedDump);
                                    
                                    File.WriteAllBytes( pluginPatchDumpFile, sortedPluginDump);
                                    File.WriteAllBytes( presetDataFile, preset.PresetData);
                                        
                                    if (File.Exists(originalPatchDump))
                                    {memory.Load(originalPatchDump);
                                        var sortedOriginal = SortDump(File.ReadAllBytes(originalPatchDump));
                                        File.WriteAllBytes(originalPatchDump+".sorted", sortedOriginal);

                                    }
                                        
                                    File.WriteAllText(structureDumpFile, rolandScript.Dump(memory));
                                    
                                    if (!sortedDumpEqual || !sortedOriginalDumpEqual)
                                    {
                                        
                                        

                                    
                                        
                                    
                                        throw new Exception($"Dumps not equal. sortedDumpEqual {sortedDumpEqual} sortedOriginalDumpEqual {sortedOriginalDumpEqual}");
                                
                                    }

                                    if (!File.Exists(okPluginPatchDumpFile))
                                    {
                                        File.Copy(pluginPatchDumpFile, okPluginPatchDumpFile);
                                    }
                                }
                                catch (Exception e)
                                {
                                    var structureDumpFile  = outputFile + ".structure";
                                    File.WriteAllText(structureDumpFile, rolandScript.Dump(memory));
                                    throw e;
                                }

                                Debug.WriteLine($"{sw.Elapsed.TotalMilliseconds} ms");

                                
                                
                            }
                        }
                    }
                }

            }

           // return;

        
           
           

        }

        static RolandScript ParseScript(string fileName, string callbackHook = "")
        {
            var rootDocument = XDocument.Parse(File.ReadAllText(fileName), LoadOptions.SetLineInfo);
            var rootNode = rootDocument.Element("script");
            
            var root = new RolandScript(rootNode);
            root.PrepareCallbacks(callbackHook);
            root.Parse();

            return root;
        }

        
        static bool Test(string chunkFile)
        {
            var compareError = false;
            var memoryAddressUnreadError = false;
            
            var memory = new RolandMemory();
            List<string> compareFileAgainstMemoryErrors = new List<string>();
            memory.Load(chunkFile);
            memory.ReadFile(chunkFile+".preset");
            
            
            var scriptFile = Path.Combine(Path.GetDirectoryName(chunkFile), "Script.xml");
            var exportConfigFile = Path.Combine(Path.GetDirectoryName(chunkFile), "ExportConfig.json");
            var callbacksFile = Path.Combine(Path.GetDirectoryName(chunkFile), "callback.txt");
            var suffixFile = Path.Combine(Path.GetDirectoryName(chunkFile), "suffix.data");
            var rootDocument = XDocument.Parse(File.ReadAllText(scriptFile), LoadOptions.SetLineInfo);

            var rootNode = rootDocument.Element("script");

            var root = new RolandScript(rootNode);

            if (File.Exists(callbacksFile))
            {
                var callbackDevice = File.ReadAllText(callbacksFile).Trim();
                root.PrepareCallbacks(callbackDevice);
            }
           
            root.Parse();

            if (File.Exists(chunkFile + ".preset"))
            {

                foreach (var subStruct in root.Structs)
                {
                    if (subStruct.Name == "fm")
                    {
                        //compareFileAgainstMemoryErrors = subStruct.CompareAgainstFileMemory(memory);
                    }
                }
            }

            var exportConfig = new RolandExportConfig();
            
            if (File.Exists(exportConfigFile))
            {
                exportConfig = JsonConvert.DeserializeObject<RolandExportConfig>(File.ReadAllText(exportConfigFile));
            }
            
            foreach (var subStruct in root.Structs)
            {
                if (subStruct.Name == "fm")
                {
                    using (var ms = new MemoryStream())
                    {
                        subStruct.DumpToPatchFile(memory, ms, exportConfig);

                        if (File.Exists(suffixFile))
                        {
                            var suffixData = File.ReadAllBytes(suffixFile);
                            ms.Write(suffixData,0,suffixData.Length);
                        }
                        File.WriteAllBytes(chunkFile + ".patchdump", ms.ToArray());
                        SortMemoryDumpFile(chunkFile + ".patchdump", chunkFile + ".patchdump.sorted");
                    }
                }
            }
            
            SortMemoryDumpFile(chunkFile, chunkFile + ".sorted");

            var rootDump = root.Dump(memory);
            


            var csvFile = Path.ChangeExtension(chunkFile, "csv");
            var compareLog = new StringBuilder();
            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader))
            {    
                var records = csv.GetRecords<VstParameterCsv>();

                foreach (var record in records)
                {
                    var expectedVal = int.Parse(record.DisplayValue);
                    var results = root.GetVstParameterValueByName(record.Name);

                    if (results.Count == 0)
                    {
                        compareLog.AppendLine($"WARNING: Unable to find {record.Name} as value.  Expected {expectedVal} (0x{expectedVal:x8})");
                        compareError = true;
                        continue;
                    }

                    RolandValue val = null;

                    foreach (var result in results)
                    {
                        if (result.HasValue(memory))
                        {
                            if (val != null && val.StartAddress != result.StartAddress)
                            {
                                compareError = true;
                                compareLog.AppendLine(
                                    $"Got multiple results for GetValueByName({record.Name}) which all have a memory location present");
                            }
                            val = result;
                        }
                    }

                    if (val == null)
                    {
                        val = results.First();
                    }
                    

                    var actualVal = val.GetValue(memory);

                    if (expectedVal != actualVal)
                    {
                        compareLog.AppendLine($"{val.Name}: Expected {expectedVal} (0x{expectedVal:x8}) got {actualVal} (0x{actualVal:x8})");
                        compareError = true;
                    }
                    else
                    {
                        compareLog.AppendLine($"{val.Name} OK {expectedVal} (0x{expectedVal:x8})");
                    }
                    
                    
                }
            }
            
            
            
            var unreadMemory = memory.GetUnreadMemory();
            unreadMemory.Sort();
            var memoryLog = new StringBuilder();
 
            foreach (var mem in unreadMemory)
            {
                if (mem >= 0x10000000)
                {
                    continue;
                }

                var result = root.FindClosestToAddress(mem);
                
                memoryLog.AppendLine($"Unread memory 0x{mem:x8}. Closest with distance of {result.distance}: {result.closestSection.GetInfo(memory)}");
                memoryAddressUnreadError = true;
            }

            //var result2 = CompareJuno106(chunkFile, memory);
            //bool presetTestError = result2.Item1;


            if (chunkFile.Contains("PD That Pad.bin"))
            {
                memoryAddressUnreadError = true;   
            }


            //memoryAddressUnreadError = true;
            File.WriteAllText(chunkFile+".structure", rootDump);

            if (memoryAddressUnreadError || compareError ||  compareFileAgainstMemoryErrors.Count > 0)
            {
                
                Debug.WriteLine("--- BEGIN DUMP ---");
                Debug.WriteLine(rootDump);
                Debug.WriteLine("--- END DUMP ---");
                memory.Dump(chunkFile+".memdump");

                if (compareError)
                {
                    Debug.WriteLine("--- BEGIN COMPARE ERRORS ---");
                    Debug.WriteLine(compareLog.ToString());
                    Debug.WriteLine("--- END COMPARE ERRORS ---");
                }

                if (compareFileAgainstMemoryErrors.Count > 0)
                {
                    Debug.WriteLine("--- BEGIN FILE AGAINST MEMORY COMPARE ERRORS ---");
                    foreach (var i in compareFileAgainstMemoryErrors)
                    {
Debug.WriteLine(i);
                    }
                    
                    Debug.WriteLine("--- END FILE AGAINST MEMORY COMPARE ERRORS ---");
                }

                if (memoryAddressUnreadError)
                {
                    Debug.WriteLine(memoryLog.ToString());
                }

              

                return false;
            }


            return true;


        }

        public static void SortMemoryDumpFile(string input, string output)
        {
            var memMap = new Dictionary<int, int>();
            var data = File.ReadAllBytes(input);
            using (var ms = new MemoryStream(data))
            {
                while (ms.Position != ms.Length)
                {
                    var buffer = new byte[4];

                    ms.Read(buffer, 0, 4);


                    var address = BigEndian.ToInt32(buffer, 0);
                    ms.Read(buffer, 0, 4);


                    var value= BigEndian.ToInt32(buffer, 0);
                    memMap.Add(address, value);
                }
            }

            using (var ms = new MemoryStream())
            {

                var keys = memMap.Keys.ToList();
                keys.Sort();
            foreach (var dings in keys)
            {
                    ms.Write(BigEndian.GetBytes(dings),0,4);
                    ms.Write(BigEndian.GetBytes(memMap[dings]),0,4);
                }
            File.WriteAllBytes(output, ms.ToArray());
            }
        }
        
        public static byte[] SortDump(byte[] data)
        {
            var memMap = new Dictionary<int, int>();
            using (var ms = new MemoryStream(data))
            {
                while (ms.Position != ms.Length)
                {
                    var buffer = new byte[4];

                    ms.Read(buffer, 0, 4);


                    var address = BigEndian.ToInt32(buffer, 0);
                    ms.Read(buffer, 0, 4);


                    var value= BigEndian.ToInt32(buffer, 0);
                    memMap.Add(address, value);
                }
            }

            using (var ms = new MemoryStream())
            {

                var keys = memMap.Keys.ToList();
                keys.Sort();
                foreach (var dings in keys)
                {
                    ms.Write(BigEndian.GetBytes(dings),0,4);
                    ms.Write(BigEndian.GetBytes(memMap[dings]),0,4);
                }

                return ms.ToArray();
            }
        }

        static (bool, string) CompareJuno106(string presetFile, RolandMemory memory)
        {
            presetFile = presetFile + ".juno106preset";
            var testError = false;
            var sb = new StringBuilder();
            if (File.Exists(presetFile))
            {
                var presetData = File.ReadAllBytes(presetFile);

                using (var ms2 = new MemoryStream())
                using (var ms = new MemoryStream(presetData))
                {
                    while (ms.Position != ms.Length)
                    {
                        var b2 = (ms.ReadByte() << 4) ^ ms.ReadByte();
                        var b1 = (ms.ReadByte() << 4) ^ ms.ReadByte();

                        ms2.WriteByte((byte) b2);
                        ms2.WriteByte(0);
                        ms2.WriteByte((byte) b1);
                        ms2.WriteByte(0);
                    }
                    
                    File.WriteAllBytes(presetFile+".juno106processedpreset", ms2.ToArray());
                    testError = true;
                }
               
            }
            return ( testError, sb.ToString());
        }

        static (bool,string) CompareD50(string presetFile, RolandMemory memory)
        {
            presetFile = presetFile + ".d50preset";
            var testError = false;
            var sb = new StringBuilder();
            if (File.Exists(presetFile))
            {
                var pos = 0;
                var offset = 0;
                var presetData = File.ReadAllBytes(presetFile);
                var fooBitConvertMode = false;
                var bitTerrorFooStorage = new byte[3];
                var max7bitTerrorBytes = 2;
                var even = 0;

                

                foreach (var by in presetData)
                {
                    var b = by;
                    if (fooBitConvertMode)
                    {
                        if (even == 0)
                        {
                            sb.AppendLine($"Calculating 7bit stuff at #{pos}");
                            var followingByte = presetData.ElementAt(pos + 1);
                            var nextFollowingByte = presetData.ElementAt(pos + 2);

                            var result = (by << 7) ^ (followingByte);

                            if (max7bitTerrorBytes == 3)
                            {
                                result =  (result << 7) ^ nextFollowingByte;
                            }

                            bitTerrorFooStorage[0] = (byte) (result & 0xFF);
                            bitTerrorFooStorage[1] = (byte) ((result >> 8) & 0xFF);
                            bitTerrorFooStorage[2] = (byte) (result >> 16);
                          

                            
                            //sb.AppendLine($"Calculating 7bit stuff at #{pos}. This byte is 0x{by:X}, following byte is 0x{followingByte:X}, current byte is 0x{b:X} and following is 0x{followingResult:X}");
                        }
                        
                            b = bitTerrorFooStorage[even];

                        

                        even++;
                        if (even  == max7bitTerrorBytes)
                        {
                            even = 0;
                        }
                    }
                    var a = memory.Get(pos+offset);

                    if (a != b)
                    {
                        testError = true;
                    }
                    var e = (b == a) ? "Y" : " ";
                    
                    sb.AppendLine($"#{e} {pos} Patch: 0x{b:X} Mem: 0x{a:X}");
                    pos++;

                    // File structure:
                    // Upper1 Partial (64 bytes)
                    // Upper2 Partial (64 bytes)
                    // Lower1 Partial (64 bytes)
                    // Lower2 Partial (64 bytes)
                    // Tone1 (64 bytes)
                    // Tone2 (64 bytes)
                    // Patch (64 bytes)
                    
                    
                    
                    if (pos == 128)
                    {
                        offset = 64;
                    }

                    if (pos == 256)
                    {
                        offset = -128;
                    }

                    if (pos == 320)
                    {
                        offset = 0;
                    }

                    if (pos == 384)
                    {
                        fooBitConvertMode = true;
                    }

                    if (pos == 396)
                    {
                        max7bitTerrorBytes = 3;
                    }

                    if (pos == 384 + 18)
                    {
                        fooBitConvertMode = false;
                    }

                }
            }

            return ( testError, sb.ToString());
        }

       
        
        static List<RolandStructOld> ParseNode(XElement node)
        {
            var structs = new List<RolandStructOld>();

            foreach (var element in node.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "struct":
                        structs.Add(ParseStruct(element));
                        break;
                }
            }

            return structs;

           
        }

        static RolandStructOld ParseStruct(XElement node)
        {
            var sStruct = new RolandStructOld();

            foreach (var element in node.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "type":
                        sStruct.Type = element.Value;
                        break;
                    case "name":
                        sStruct.Name = element.Value;
                        break;
                    case "address":
                        sStruct.Address = HexToInt(element.Value);
                        Debug.WriteLine($"Converted {element.Value} to {sStruct.Address}");
                        break;
                    default:
                        Debug.WriteLine(
                            $"ParseStruct: Unknown type {element.Name} at {((IXmlLineInfo) element).LineNumber}");
                        break;
                }
            }

            sStruct.RolandStructTypeOld = GetOrCreateByTypeName(sStruct.Type);
            return sStruct;
        }

        static RolandStructTypeOld GetOrCreateByTypeName(string typeName)
        {
            if (!_structTypes.ContainsKey(typeName))
            {
                _structTypes.Add(typeName, new RolandStructTypeOld() {Type = typeName});
            }

            return _structTypes[typeName];
        }

        static void ParseStructType(XElement node)
        {
            var type = node.Element("type");

            if (type == null)
            {
                Debug.WriteLine($"Error: structType has no type element?");
                return;
            }

            var structType = GetOrCreateByTypeName(type.Value);

            foreach (var element in node.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "type":
                        // We already got this
                        break;
                    case "name":
                        structType.Name = element.Value;
                        if (element.Value != "$name")
                        {
                            Debug.WriteLine(
                                $"structType {structType.Type} has a real name {element.Value} and not $name");
                        }

                        break;
                    case "address":
                        if (element.Value != "$address")
                        {
                            Debug.WriteLine(
                                $"structType {structType.Type} has a real address {element.Value} and not $address");
                        }

                        break;
                    case "size":
                        structType.OverrideSize = HexToInt(element.Value);
                        break;
                    case "struct":
                        structType.Structs.Add(ParseStruct(element));
                        break;
                    case "value":
                        structType.Values.Add(ParseValue(element));
                        break;
                    default:
                        Debug.WriteLine(
                            $"ParseStructType: Unknown type {element.Name} at {((IXmlLineInfo) element).LineNumber}");
                        break;
                }
            }
        }

        static RolandValueOld ParseValue(XElement node)
        {
            var value = new RolandValueOld();

            var setElements = new List<string>();

            foreach (var element in node.Elements())
            {
                if (setElements.Contains(element.Name.ToString()))
                {
                    Debug.WriteLine(
                        $"ParseValue: Type {element.Name} is already added at {((IXmlLineInfo) element).LineNumber}");
                }

                switch (element.Name.ToString())
                {
                    case "type":
                        value.Type = element.Value;
                        valueTypes.Add(value.Type);
                        setElements.Add("type");
                        break;
                    case "name":
                        value.Name = element.Value;
                        setElements.Add("name");
                        break;
                    case "range":
                        value.Range = element.Value;
                        setElements.Add("range");
                        break;
                    case "default":
                        value.Default = element.Value;
                        setElements.Add("default");
                        break;
                    case "size":
                        value.OverrideSize = HexToInt(element.Value);
                        setElements.Add("size");
                        break;
                    case "address":
                        value.Addresses.Add(HexToInt(element.Value));
                        break;
                    default:
                        Debug.WriteLine(
                            $"ParseValue: Unknown type {element.Name} at {((IXmlLineInfo) element).LineNumber}");
                        break;
                }
            }

            if (value.Type == null)
            {
                value.Type = "bool";
            }

            return value;
        }

        static int HexToInt(string value)
        {
            var hexValuesSplit = value.Trim().Split(' ').ToList();

            using (var ms = new MemoryStream())
            {
                if (hexValuesSplit.Count == 1)
                {
                    hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").Prepend("00").ToList();
                    Debug.WriteLine($"Warning: Appending 2x 00 to {value}");
                }

                if (hexValuesSplit.Count == 2)
                {
                    hexValuesSplit = hexValuesSplit.Prepend("00").Prepend("00").ToList();
                    Debug.WriteLine($"Warning: Appending 2x 00 to {value}");
                }

                if (hexValuesSplit.Count == 3)
                {
                    hexValuesSplit = hexValuesSplit.Prepend("00").ToList();
                    Debug.WriteLine($"Warning: Appending 1x 00 to {value}");
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

                return ints[0] * 128 * 128* 128+ ints[1] * 128* 128 + ints[2] * 128+ ints[3];

            }
        }
    }

    public class RolandStructOld
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int Address { get; set; }

        public int Size { get; private set; }

        public RolandStructTypeOld RolandStructTypeOld { get; set; }

        public List<RolandStructOld> Structs { get; } = new List<RolandStructOld>();
        public List<RolandValueOld> Values { get; } = new List<RolandValueOld>();

        public RolandStructOld Realize(bool recalculateAddresses = false)
        {
            var str = new RolandStructOld();

            if (RolandStructTypeOld.Name == "$name")
            {
                str.Name = Name;
            }
            else
            {
                str.Name = RolandStructTypeOld.Name;
            }

            str.Type = Type;
            str.Address = Address;

            /*if (RolandStructType.OverrideSize != null)
            {
                str.Size = (int)RolandStructType.OverrideSize;
            }
            else
            {*/
                str.Size = RolandStructTypeOld.CalculateSize();
            //}

            int lastOffset = 0;

            foreach (var subStr in RolandStructTypeOld.Structs)
            {
                var realizedSubstr = subStr.Realize();
                if (recalculateAddresses)
                {
                    realizedSubstr.Address = lastOffset;
                    lastOffset += realizedSubstr.Size;
                }
                str.Structs.Add(realizedSubstr);
            }

            foreach (var val in RolandStructTypeOld.Values)
            {
                str.Values.Add(val.Realize());
            }

            return str;
        }

        public override string ToString()
        {
            return $"<{Type}>{Name} {Address:x8}";
        }

        public int CalculateSize()
        {
            return RolandStructTypeOld.CalculateSize();
        }

        public void Dump(int baseAddress, int indent, RolandMemory memory)
        {
            string pad = "";
            for (var i = 0; i < indent; i++)
            {
                pad = pad + "  ";
            }

            var startAddress = baseAddress + Address;
            var endAddress = startAddress + Size;

            Debug.WriteLine($"{pad}{Name} ({Type})".PadRight(60) +
                            $"0x{startAddress:x8} - 0x{endAddress:x8} (Len 0x{Size:x8})");

            foreach (var str in Structs)
            {
                str.Dump(startAddress, indent + 1, memory);
            }

            foreach (var val in Values)
            {
                val.Dump(startAddress, indent + 1, memory);
                if (val.Addresses.Count == 0)
                {
                    startAddress = startAddress + val.Size;
                }
            }
        }
    }

    public class RolandStructTypeOld
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int? OverrideSize { get; set; }
        public int Size { get; private set; }

        public List<RolandStructOld> Structs { get; } = new List<RolandStructOld>();
        public List<RolandValueOld> Values { get; } = new List<RolandValueOld>();


        public int CalculateSize()
        {
            if (OverrideSize != null)
            {
                //return (int) OverrideSize;
            }

            int sum = 0;
            RolandStructOld biggestOffset = null;
            RolandValueOld biggestOffsetValueOld = null;
            
            foreach (var str in Structs)
            {
                if (biggestOffset == null)
                {
                    biggestOffset = str;
                }

                if (str.Address > biggestOffset.Address)
                {
                    biggestOffset = str;
                        
                }
                
            }

            if (biggestOffset != null)
            {
                sum += biggestOffset.Address + biggestOffset.CalculateSize();
            }

            foreach (var val in Values)
            {
                if (val.Addresses.Count == 0)
                {
                    sum += val.CalculateSize();
                    continue;
                }
                
                if (biggestOffsetValueOld == null)
                {
                    biggestOffsetValueOld = val;
                }

                var firstAddress = val.GetSmallestAddress();
                var size = val.CalculateSize();

                if (firstAddress + size > sum)
                {
                    sum = firstAddress + size;
                }

                
                
                
            }

            return sum;
        }
    }


   public class RolandValueOld
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Range { get; set; }
        public string Default { get; set; }
        public int? OverrideSize { get; set; }

        /// <summary>
        /// The size in nibbles?
        /// </summary>
        public int Size { get; private set; }

        public List<int> Addresses { get; } = new List<int>();

        public override string ToString()
        {
            return $"<{Type}>{Name}";
        }

        public RolandValueOld Realize()
        {
            var val = new RolandValueOld();
            val.Type = Type;
            val.Name = Name;
            val.Range = Range;
            val.Default = Default;
            val.Size = CalculateSize();

            foreach (var addr in Addresses)
            {
                val.Addresses.Add(addr);
            }

            return val;
        }

        public int GetTypeSize()
        {
            int baseSize = 0;
            switch (Type)
            {
                case "int1x7":
                    baseSize = 1;
                    break;
                case "bool":
                    baseSize = 1;
                    break;
                case "int4x4":
                    baseSize = 4;
                    break;
                case "int2x4":
                    baseSize = 2;
                    break;
                case "int8x4":
                    baseSize = 8;
                    break;
                case "int3x4":
                    baseSize = 3;
                    break;
            }


            return baseSize;
        }

        public int CalculateSize()
        {
            if (OverrideSize != null)
            {
                return (int) OverrideSize;
            }

            if (Addresses.Count > 0)
            {
                var b = GetBiggestAddress();
                var s = GetSmallestAddress();
                var ts = GetTypeSize();
                return (GetBiggestAddress() + GetTypeSize() - GetSmallestAddress());
            }

            return GetTypeSize();
        }

        public int GetSmallestAddress()
        {
            int smallestAddress = Int32.MaxValue;
            foreach (var address in Addresses)
            {
                if (address < smallestAddress)
                {
                    smallestAddress = address;
                }
            }

            return smallestAddress;
        }
        
        public int GetBiggestAddress()
        {
            int biggestAddress = 0;
            foreach (var address in Addresses)
            {
                if (address > biggestAddress)
                {
                    biggestAddress = address;
                }
            }

            return biggestAddress;
        }

        public void Dump(int baseAddress, int indent , RolandMemory memory)
        {
            if (Addresses.Count == 0)
            {
                DumpInternal(baseAddress, 0, indent, memory);
            }
            else
            {
                foreach (var addr in Addresses)
                {
                    DumpInternal(baseAddress, addr, indent, memory);
                }
            }
        }

        private string GetValue(int startAddress, RolandMemory memory)
        {
            switch (Type)
            {
                case "int1x7":
                    if (!memory.Has(startAddress+3))
                    {
                        return null;
                    }
                    
                    // Dummy reads
                   /* memory.Get(startAddress);
                    memory.Get(startAddress + 1);
                    memory.Get(startAddress + 2);*/
                    
                    return $"0x{memory.Get(startAddress+3):X} ({memory.Get(startAddress+3)})"; 
                case "int8x4":
                    if (!memory.Has(startAddress) || !memory.Has(startAddress + 1) ||  !memory.Has(startAddress + 2) ||  !memory.Has(startAddress + 3))
                    {
                        return null;
                    }

                    var val2 = memory.Get(startAddress+3) * 256 * 256 * 256 + memory.Get(startAddress + 2) * 256 * 256 + memory.Get(startAddress +1)*256 + memory.Get(startAddress); 
                    return $"0x{memory.Get(startAddress+3):X} 0x{memory.Get(startAddress+2):X} 0x{memory.Get(startAddress+1):X} 0x{memory.Get(startAddress+0):X} ({val2})"; 
           
                case "int4x4":
                    case "int3x4":
                    if (!memory.Has(startAddress) || !memory.Has(startAddress + 1))
                    {
                        return null;
                    }
                    
                    // Dummy reads
                    //memory.Get(startAddress);
                    //memory.Get(startAddress + 1);

                    var val = memory.Get(startAddress+0) * 256 + memory.Get(startAddress + 1);
                    return $"0x{memory.Get(startAddress+0):X} 0x{memory.Get(startAddress+1):X} ({val})"; 
                case "int2x4":
                    if (!memory.Has(startAddress))
                    {
                        return null;
                    }
                    
                    // Dummy reads
                   /* memory.Get(startAddress);
                    memory.Get(startAddress + 1);
                    memory.Get(startAddress + 2);*/
                    
                    return $"0x{memory.Get(startAddress):X} ({memory.Get(startAddress)})"; 
                case "stringNx4":
                    var str = "\"";
                    var hex = "";
                    for (var i = 0; i < Size; i+=4)
                    {
                        var address = startAddress + i;
                        if (!memory.Has(address) || !memory.Has(address + 1))
                        {
                            return null;
                        }
                        
                        memory.Get(address);
                        memory.Get(address+1);
                        
                        hex = hex + $"0x{memory.Get(address+1):X} 0x{memory.Get(address+0):X} ";
                        str = str + (char) memory.Get(address+1) +(char) memory.Get(address+0);
                    }

                    str = str + "\"";

                    return $"{hex} {str}";
                case "bool":
                    if (!memory.Has(startAddress))
                    {
                        return null;
                    }

                    // Dummy reads
                    /*memory.Get(startAddress);
                    memory.Get(startAddress + 1);
                    memory.Get(startAddress + 2);*/
                    
                    return $"0x{memory.Get(startAddress):X} ({memory.Get(startAddress)})"; 
            }

            return "<not implemented>";
        }

        private void DumpInternal(int baseAddress, int address, int indent, RolandMemory memory)
        {
            string pad = "";
            for (var i = 0; i < indent; i++)
            {
                pad = pad + "  ";
            }

            var startAddress = baseAddress + address;
            var endAddress = startAddress + Size;

            var value = GetValue(startAddress, memory);
            var dataStr = "";

            if (value != null)
            {
                dataStr = $"Data: {value}";
            }
            

            Debug.WriteLine($"{pad} {Name} ({Type})".PadRight(60) +
                            $"0x{startAddress:x8} - 0x{endAddress:x8} (Len 0x{Size:x8}) {dataStr}");
        }
    }

    
}