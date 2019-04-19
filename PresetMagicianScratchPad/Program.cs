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

            var doStructureDump = false;
           
            foreach (var configFile in Directory.EnumerateFiles(
                @"C:\Users\Drachenkatze\Documents\PresetMagician\TestVstHost\Patches", "ExportConfig.json",
                SearchOption.AllDirectories))
            {
                if (!configFile.Contains("SYSTEM-100"))
                {
                    continue;
                }

                var exportConfig = JsonConvert.DeserializeObject<RolandExportConfig>(File.ReadAllText(configFile));

                var serviceLocator = ServiceLocator.Default;

                FrontendInitializer.RegisterTypes(serviceLocator);
                FrontendInitializer.Initialize(serviceLocator);

                var remoteVstService = serviceLocator.ResolveType<RemoteVstService>();

                var plugin = new Plugin {PluginLocation = new PluginLocation {DllPath = exportConfig.Plugin}};

                var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

                pluginInstance.LoadPlugin().Wait();

                var koaParser = new KoaBankFileParser(exportConfig.KoaFileHeader, exportConfig.PresetNameLength,
                    exportConfig.PresetLength);

                var rolandScript = ParseScript(exportConfig);
                var fooViz = new FooViz();
                // rolandScript.ExportNode.Save(exportConfig.ScriptFile+".modified");

                var hadOriginal = false;
                foreach (var patchDirectory in exportConfig.PresetDirectories)
                {
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

                            var sortedStructs =
                                (from str in structs orderby str.Length descending, str.Start select str);

                            foreach (var sortedStruct in sortedStructs)
                            {
                                fooViz.AddStructure(sortedStruct.Start, sortedStruct.Length, sortedStruct.Content);
                            }

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
                                    var sortedDump = RolandPatchDump.SortDump(ms.ToArray());

                                    byte[] sortedPluginDump;
                                    if (File.Exists(okPluginPatchDumpFile))
                                    {
                                        sortedPluginDump = RolandPatchDump.SortDump(File.ReadAllBytes(okPluginPatchDumpFile));
                                    }
                                    else
                                    {
                                        pluginInstance.SetChunk(sortedDump, false);

                                        sortedPluginDump = RolandPatchDump.SortDump(pluginInstance.GetChunk(false));
                                    }

                                    var pluginDumpVsGeneratedDumpErrors =
                                        RolandPatchDump.ComparePatchDumps(sortedPluginDump, sortedDump, rolandScript);
                                    var sortedDumpEqual = pluginDumpVsGeneratedDumpErrors.Count == 0;
                                    
                                    var sortedOriginalDumpEqual = true;
                                    var sortedOriginalDumpEqualToPluginDump = false;
                                    if (File.Exists(originalPatchDump))
                                    {
                                        var sortedOriginal = RolandPatchDump.SortDump(File.ReadAllBytes(originalPatchDump));
                                        sortedOriginalDumpEqual = sortedDump.SequenceEqual(sortedOriginal);
                                        sortedOriginalDumpEqualToPluginDump = sortedPluginDump.SequenceEqual(sortedOriginal);

                                        hadOriginal = true;
                                    }
                                   
                                    var patchDumpFile = outputFile + ".patchdump";
                                    var structureDumpFile = outputFile + ".structure";
                                    var pluginPatchDumpFile = outputFile + ".pluginpatchdump";


                                    var presetDataFile = outputFile + ".presetdata";
                                    File.WriteAllBytes(patchDumpFile, sortedDump);

                                    File.WriteAllBytes(pluginPatchDumpFile, sortedPluginDump);
                                    File.WriteAllBytes(presetDataFile, preset.PresetData);

                                    if (File.Exists(originalPatchDump))
                                    {
                                        memory.Load(originalPatchDump);
                                        var sortedOriginal =RolandPatchDump.SortDump(File.ReadAllBytes(originalPatchDump));
                                        File.WriteAllBytes(originalPatchDump + ".sorted", sortedOriginal);
                                    }


                                    if (doStructureDump || (!sortedDumpEqual || !sortedOriginalDumpEqual))
                                    {
                                        File.WriteAllText(structureDumpFile, rolandScript.Dump(memory));
                                    }

                                    if ((!sortedDumpEqual || !sortedOriginalDumpEqual) && !sortedOriginalDumpEqualToPluginDump)
                                    {
                                        // Last resort: Compare the plugin dump against the sorted original dump because of "if float goes in, float comes out slightly different"

                                        Debug.WriteLine(string.Join(Environment.NewLine, pluginDumpVsGeneratedDumpErrors));

                                        throw new Exception(
                                            $"Dumps not equal. sortedDumpEqual {sortedDumpEqual} sortedOriginalDumpEqual {sortedOriginalDumpEqual}");
                                    }

                                    if (!File.Exists(okPluginPatchDumpFile))
                                    {
                                        File.Copy(pluginPatchDumpFile, okPluginPatchDumpFile);
                                    }
                                }
                                catch (Exception e)
                                {
                                    var structureDumpFile = outputFile + ".structure";
                                    File.WriteAllText(structureDumpFile, rolandScript.Dump(memory));
                                    throw e;
                                }

                                Debug.WriteLine($"{sw.Elapsed.TotalMilliseconds} ms");
                            }
                        }
                    }
                }

                if (!hadOriginal)
                {
                    throw new Exception($"Had no original to test against for config {configFile}");
                }
            }

            // return;
        }

        static RolandScript ParseScript(RolandExportConfig config)
        {
            var rootDocument = XDocument.Parse(File.ReadAllText(config.ScriptFile), LoadOptions.SetLineInfo);
            var rootNode = rootDocument.Element("script");

            var root = new RolandScript(rootNode);
            root.Config = config;
            root.Parse();

            return root;
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


                    var value = BigEndian.ToInt32(buffer, 0);
                    memMap.Add(address, value);
                }
            }

            using (var ms = new MemoryStream())
            {
                var keys = memMap.Keys.ToList();
                keys.Sort();
                foreach (var dings in keys)
                {
                    ms.Write(BigEndian.GetBytes(dings), 0, 4);
                    ms.Write(BigEndian.GetBytes(memMap[dings]), 0, 4);
                }

                File.WriteAllBytes(output, ms.ToArray());
            }
        }

        
    }
}