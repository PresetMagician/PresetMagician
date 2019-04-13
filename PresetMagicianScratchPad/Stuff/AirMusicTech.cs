using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Catel.IoC;
using Catel.IO;
using Catel.Reflection;
using GSF;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Utils;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using Path = System.IO.Path;

namespace PresetMagicianScratchPad.Stuff
{
    public class TfxDummy : Tfx
    {
        public override byte[] BlockMagic { get; } = {0xFF, 0xFF, 0xFF, 0xFF};

        public override byte[] GetEndChunk()
        {
            throw new NotImplementedException();
        }
    }
    
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class AirMusicTech
    {
        public static void CreatePresetParserCode()
        {
            var testSetups = AirMusicTech.GetTestSetups();

            var ppDirectory =
                @"C:\Users\Drachenkatze\source\repos\presetmagician\PresetMagician.VendorPresetParser\AIRMusicTechnology";
            var ppTemplate = Path.Combine(ppDirectory, "AirTech_Template.txt");
            var tfxDirectory = Path.Combine(ppDirectory, "Tfx");
            var tfxTemplate = Path.Combine(tfxDirectory, "TfxTemplate.txt");

            foreach (var testSetup in testSetups)
            {
                var parser = AirMusicTech.GetTfxByName(testSetup.TfxParserType);
                if (parser != null)
                {
                    continue;
                }

                var result = AirMusicTech.GetChunkFromPlugin(testSetup);
                var chunkResult = AirMusicTech.AnalyzeChunk(result.Item1, testSetup.TfxParserType);
                var tfxResult = AirMusicTech.AnalyzeTfx(Path.Combine(testSetup.PresetDirectory, "Default.tfx"));
                if (chunkResult.HasMidi && chunkResult.HasPresetNameAtEnd
                                        && chunkResult.HasEmptyMidi &&
                                        chunkResult.NumParameters == tfxResult.NumParameters)
                {
                    //if (chunkResult.EndChunk.SequenceEqual(PresetMagician.VendorPresetParser.VendorResources.AirFxSuiteEndChunk))
                    var className = testSetup.TfxParserType.Replace("Tfx", "");
                    var pluginDir = testSetup.PresetDirectory
                        .Replace(@"C:\Program Files (x86)\AIR Music Technology\", "").Replace(@"\Presets", "");
                    var text = File.ReadAllText(ppTemplate);
                    var outputFile = Path.Combine(ppDirectory, $"AirTech_{className}.cs");
                    text = text.Replace("--PPCLASSNAME--", className);
                    text = text.Replace("--PLUGINID--", result.Item2.ToString());
                    text = text.Replace("--PLUGINDIR--", pluginDir);
                    text = text.Replace("--TFXPARSER--", testSetup.TfxParserType);

                    if (!File.Exists(outputFile))
                    {
                        File.WriteAllText(outputFile, text);
                    }
                    else
                    {
                        throw new Exception("Output file exists");
                    }
                    
                    var text2 = File.ReadAllText(tfxTemplate);
                    var outputFile2 = Path.Combine(tfxDirectory, $"{testSetup.TfxParserType}.cs");
                    
                    text2 = text2.Replace("--TFXPARSER--", testSetup.TfxParserType);

                    for (var i = 0; i < chunkResult.BlockMagic.Length; i++)
                    {
                        text2 = text2.Replace($"--BLOCKMAGIC{i+1}--", string.Format("{0:x2}",chunkResult.BlockMagic[i]));
                    }
                    
                    text2 = text2.Replace("--MAGICPLUGINNAME--", chunkResult.PluginName);
                    
                    if (!File.Exists(outputFile2))
                    {
                        File.WriteAllText(outputFile2, text2);
                    }
                    else
                    {
                        throw new Exception("Output file 2 exists");
                    }

                }
                Console.WriteLine($"HasMidi: {chunkResult.HasMidi}");
                Console.WriteLine($"HasPresetNameAtEnd: {chunkResult.HasPresetNameAtEnd}");
                Console.WriteLine($"PluginName: {chunkResult.PluginName}");
                Console.WriteLine($"BlockMagic: {StringUtils.ByteArrayToHexString(chunkResult.BlockMagic)}");
                
//                @"C:\Users\Drachenkatze\Documents\PresetMagician\ScratchPad\test.bin");
            }
        }
        public static void TestLoadInPlugin(AirMusicTechTestSetup testSetup)
        {
            var directory = GetTestDirectory(testSetup);
            var files = Directory.EnumerateFiles(directory, "*.airtech.chunk", SearchOption.AllDirectories).ToList();

            if (files.Count == 0)
            {
                throw new Exception("No presets found");
            }
            TestLoadInPlugin(testSetup, files.ToList());
        }

        public static void AnalyzeChunk(string file)
        {
            AnalyzeChunk(File.ReadAllBytes(file), file);
        }

        public static ChunkAnalysisResult AnalyzeChunk(byte[] data, string file)
        {
            var chunkAnalysisResult = new ChunkAnalysisResult();
            var intBuffer = new byte[4];

            Console.WriteLine($"Chunk Info for {file}");
            Console.WriteLine(
                "=======================================================================================================");
            Console.WriteLine("");
           

            using (var ms = new MemoryStream(data))
            {
                ms.Read(intBuffer, 0, 4);

                var totalLength = LittleEndian.ToInt32(intBuffer, 0);
                var lengthDifference = ms.Length - totalLength - 16;
                Console.WriteLine(
                    $"Content total length: {totalLength} (length difference: {lengthDifference}, should be 0)");

                ms.Seek(12, SeekOrigin.Current);

                ms.Read(intBuffer, 0, 4);
                var numParameters = LittleEndian.ToInt32(intBuffer, 0);
                Console.WriteLine($"Num parameters: {numParameters}");

                chunkAnalysisResult.NumParameters = numParameters;
                ms.Seek(numParameters * 4, SeekOrigin.Current);

                var currentBlock = 0;
                while (true)
                {
                    currentBlock++;
                    if (ms.Position == ms.Length)
                    {
                        break;
                        
                    }

                    ms.Read(intBuffer, 0, 4);
                    var blockLength = LittleEndian.ToInt32(intBuffer, 0);
                    var startPos = ms.Position;
                    if (ms.Position + blockLength > ms.Length)
                    {
                        Console.WriteLine(
                            $"Error reading block starting at {ms.Position - 4}: Block length exceeds file size");
                        break;
                    }

                    if (blockLength == 0)
                    {
                        var unknownBlockStart = ms.Position;
                        ms.Read(intBuffer, 0, 4);
                        
                        
                        
                        var subBlockLength = LittleEndian.ToInt32(intBuffer, 0);
                        var blockBuffer = new byte[subBlockLength];

                        if (subBlockLength == 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine($"Info for block #{currentBlock} starting at {unknownBlockStart} length {subBlockLength} end {unknownBlockStart+subBlockLength}");
                            Console.WriteLine($"-------------------------------");
                            Console.WriteLine("Empty unknown data block");
                            continue;
                            
                        }
                        ms.Seek(-4, SeekOrigin.Current);
                        ms.Read(blockBuffer, 0, subBlockLength);
                        
                        Console.WriteLine();
                        Console.WriteLine($"Info for block #{currentBlock} starting at {unknownBlockStart} length {subBlockLength} end {unknownBlockStart+subBlockLength}");
                        Console.WriteLine($"-------------------------------");
                        Console.WriteLine("Unknown data block");
                    }
                    else
                    {

                        var blockBuffer = new byte[blockLength];


                        ms.Read(blockBuffer, 0, blockLength);


                        AnalyzeChunkSubBuffer(currentBlock, (int) startPos, blockBuffer, chunkAnalysisResult);
                    }


                }
            }

            return chunkAnalysisResult;
        }

        public static void AnalyzeChunkSubBuffer(int currentBlock, int startPos, byte[] data, ChunkAnalysisResult chunkAnalysisResult)
        {
            var intBuffer = new byte[4];
            var allHigh = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            using (var ms2 = new MemoryStream(data))
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Info for block #{currentBlock} starting at {startPos} length {data.Length} end {startPos+data.Length}");
                        Console.WriteLine($"-------------------------------");
                        var blockMagic = new byte[4];
                        ms2.Read(blockMagic, 0, 4);

                        chunkAnalysisResult.BlockMagic = blockMagic;
                        Console.WriteLine($"Guessed block magic: {StringUtils.ByteArrayToHexString(blockMagic)}");
                        ms2.Read(intBuffer, 0, 4);
                        var pluginNameLength = BigEndian.ToInt32(intBuffer, 0);

                        if (pluginNameLength < ms2.Length - ms2.Position)
                        {
                            var pluginNameBuf = new byte[pluginNameLength];
                            ms2.Read(pluginNameBuf, 0, pluginNameLength);

                            if (chunkAnalysisResult.PluginName == "")
                            {
                                chunkAnalysisResult.PluginName = Encoding.BigEndianUnicode.GetString(pluginNameBuf);
                            }
                            Console.WriteLine($"Guessed plugin name: {Encoding.BigEndianUnicode.GetString(pluginNameBuf)}");
                        }
                        else
                        {
                            ms2.Seek(-4, SeekOrigin.End);
                            var buf3 = new byte[4];
                            ms2.Read(buf3, 0, 4);

                            if (buf3.SequenceEqual(allHigh))
                            {
                                chunkAnalysisResult.EndChunk = ms2.ToByteArray();
                                Console.WriteLine("Probably the page stuff (end chunk?)");
                            }
                            else
                            {
                                ms2.Seek(0, SeekOrigin.Begin);
                               
                                var presetBuf = new byte[data.Length];
                                ms2.Read(presetBuf, 0, data.Length);
                                chunkAnalysisResult.HasPresetNameAtEnd = true;
                                Console.WriteLine($"Probably the current preset name? Decoded: {Encoding.ASCII.GetString(presetBuf)}");
                            }
                            return;
                        }

                        var buf2 = new byte[4];
                        ms2.Read(buf2, 0, 4);
                        var empty = new byte[] {0x00, 0x00, 0x00, 0x00};
                        
                        if (buf2.SequenceEqual(empty))
                        {
                            Console.WriteLine("Got 4x 0x00 after the plugin name");
                        }
                        else
                        {
                            Console.WriteLine("Did NOT get 4x 0x00 after the plugin name");
                        }

                        ms2.Read(intBuffer, 0, 4);

                        if (intBuffer.SequenceEqual(allHigh))
                        {
                            ms2.Read(intBuffer, 0, 4);

                            if (intBuffer.SequenceEqual(Tfx.DEADBEEF))
                            {
                                Console.WriteLine("Empty block without data");
                            }
                            else
                            {
                                var midiBlockLength = BigEndian.ToInt32(intBuffer, 0);
                                if (ms2.Position + midiBlockLength + 4 == ms2.Length)
                                {
                                    chunkAnalysisResult.HasMidi = true;

                                    if (midiBlockLength == 4)
                                    {
                                        chunkAnalysisResult.HasEmptyMidi = true;
                                    }
                                    Console.WriteLine("Is probably a midi buffer");
                                }
                                else
                                {
                                    Console.WriteLine("Unknown block?");
                                }
                            }
                        }
                        else
                        {
                            var subBlockLength = BigEndian.ToInt32(intBuffer, 0);

                            if (ms2.Length - ms2.Position - 4 == subBlockLength)
                            {
                                Console.WriteLine($"Sub block length matches length {subBlockLength}");
                            }
                            else
                            {
                                Console.WriteLine("Sub block length does NOT match length");
                            }
                        }


                        var buf = new byte[4];
                        ms2.Seek(-4, SeekOrigin.End);
                        ms2.Read(buf, 0, 4);

                        if (buf.SequenceEqual(Tfx.DEADBEEF))
                        {
                            Console.WriteLine("Ends with DEADBEEF");
                        }
                        else
                        {
                            Console.WriteLine("Does not end with DEADBEEF");
                        }

                    }
        }

        public static TfxAnalysisResult AnalyzeTfx(string file)
        {
            var analysisResult = new TfxAnalysisResult(); 
            var directory = Path.GetDirectoryName(file);
            var filename = Path.GetFileName(file);
            var tfx = new TfxDummy();
            tfx.Parse(directory, filename);

            Console.WriteLine($"RFX Info for {file}");
            Console.WriteLine(
                "=======================================================================================================");
            Console.WriteLine("");
            Console.WriteLine($"Num Parameters: {tfx.Parameters.Count}");

            analysisResult.NumParameters = tfx.Parameters.Count;
            foreach (var block in tfx.WzooBlocks)
            {
                Console.WriteLine(
                    $"Block {Encoding.ASCII.GetString(block.DeviceType)} Config Type {Encoding.ASCII.GetString(block.ConfigType)}");
                Console.WriteLine(
                    "=======================================================================================================");

                Console.WriteLine($"Length: {block.BlockLength} bytes (including config type)");
                Console.WriteLine($"Data Length: {block.BlockData.Length} bytes");

                var isMagicblock = tfx.ContainsMagicBlock(block);
                var magicBlock = tfx.ParseMagicBlock(block);
                Console.WriteLine($"Magic Block: {isMagicblock}");
                Console.WriteLine($"Sub-Block Data Length: {magicBlock.BlockData.Length}");

                if (isMagicblock)
                {
                    Console.WriteLine($"Magic Plugin Data: {Encoding.BigEndianUnicode.GetString(magicBlock.PluginName)}");
                }
            }
            
            
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);

            return analysisResult;
        }

        public static void TestLoadInPlugin(AirMusicTechTestSetup testSetup, List<string> files)
        {
            var serviceLocator = ServiceLocator.Default;

            FrontendInitializer.RegisterTypes(serviceLocator);
            FrontendInitializer.Initialize(serviceLocator);

            var remoteVstService = serviceLocator.ResolveType<RemoteVstService>();

            var plugin = new Plugin {PluginLocation = new PluginLocation {DllPath = testSetup.PluginDll}};

            var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

            pluginInstance.LoadPlugin().Wait();

            foreach (var file in files)
            {
                Debug.WriteLine($"Loading {file} into plugin");
                pluginInstance.SetChunk(File.ReadAllBytes(file), false);
            }
        }
        
        public static (byte[], int) GetChunkFromPlugin(AirMusicTechTestSetup testSetup)
        {
            var serviceLocator = ServiceLocator.Default;

            FrontendInitializer.RegisterTypes(serviceLocator);
            FrontendInitializer.Initialize(serviceLocator);

            var remoteVstService = serviceLocator.ResolveType<RemoteVstService>();

            var plugin = new Plugin {PluginLocation = new PluginLocation {DllPath = testSetup.PluginDll}};

            var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

            pluginInstance.LoadPlugin().Wait();


            return (pluginInstance.GetChunk(false), pluginInstance.Plugin.VstPluginId);

        }

        public static string GetTestDirectory(AirMusicTechTestSetup testSetup)
        {
            var pluginName = Path.GetFileNameWithoutExtension(testSetup.PluginDll);
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $@"PresetMagician\ScratchPad\AirMusicTech\{pluginName}\");
        }

        public static void ConvertFiles(AirMusicTechTestSetup testSetup)
        {
            var outputDirectory = GetTestDirectory(testSetup);

            Directory.CreateDirectory(outputDirectory);


            foreach (var file in Directory.EnumerateFiles(testSetup.PresetDirectory, "*.tfx",
                SearchOption.AllDirectories))
            {
                Debug.WriteLine($"Converting {file}");

                var parser = GetTfxByName(testSetup.TfxParserType);
                var fileName = file.Replace(testSetup.PresetDirectory + "\\", "");

                parser.Parse(testSetup.PresetDirectory, fileName);

                var outputFile = Path.Combine(outputDirectory, fileName.Replace(".tfx", ".airtech.chunk"));
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                var data = parser.GetDataToWrite();
                File.WriteAllBytes(outputFile, data);
            }
        }

        public static Tfx GetTfxByName(string name)
        {
            var assemblyObj = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                where assembly.FullName.StartsWith("PresetMagician.VendorPresetParser")
                select assembly).First();
            var type = assemblyObj
                .GetTypes().FirstOrDefault(p =>
                    p.FullName == "PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx." +
                    name);

            if (type == null)
            {
                return null;
            }
            return (Tfx) Activator.CreateInstance(type);
        }

        public static List<AirMusicTechTestSetup> GetTestSetups()
        {
            var myActualType = typeof(AirMusicTech);
            var testSetups = new List<AirMusicTechTestSetup>();
            foreach (var member in myActualType.GetFields())
            {
                if (member.Name.StartsWith("TESTSETUP_"))
                {
                    testSetups.Add((AirMusicTechTestSetup)member.GetValue(null));
                    
                }
            }

            return testSetups;
        }
        public static string GenerateTestSetupTemplate(string pluginDll)
        {
            var pluginName = Path.GetFileNameWithoutExtension(pluginDll).Replace("_x64", "");
            var presetDirectory = "";
            foreach (var directory in Directory.EnumerateDirectories(@"C:\Program Files (x86)\AIR Music Technology"))
            {
                var dirName = Path.GetFileName(directory);
                if (dirName.Replace(" ", "").ToLower() == pluginName.ToLower())
                {
                    presetDirectory = Path.Combine(directory, "Presets");
                }
            }

            if (!Directory.Exists(presetDirectory))
            {
                presetDirectory = "NONEXISTANT";
            }
            var tpl = $@"
        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_{pluginName.ToUpper()} = new AirMusicTechTestSetup
        {{
            PluginDll = @""{pluginDll}"",
            PresetDirectory = @""{presetDirectory}"",
            TfxParserType = @""Tfx{pluginName}""
        }};";
            return tpl;
        }

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_MINIGRAND = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\MiniGrand_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Mini Grand\Presets",
            TfxParserType = "TfxMiniGrand"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_VELVET = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Velvet_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Velvet\Presets",
            TfxParserType = "TfxVelvet"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_HYBRID = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Hybrid_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets",
            TfxParserType = "TfxHybrid3"
        };
        
                // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRCHORUS = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRChorus_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Chorus\Presets",
            TfxParserType = @"TfxAIRChorus"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRCOMPRESSOR = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRCompressor_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Compressor\Presets",
            TfxParserType = @"TfxAIRCompressor"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRDIFFUSERDELAY = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRDiffuserDelay_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR DiffuserDelay\Presets",
            TfxParserType = @"TfxAIRDiffuserDelay"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRDISTORTION = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRDistortion_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Distortion\Presets",
            TfxParserType = @"TfxAIRDistortion"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRDYNAMICDELAY = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRDynamicDelay_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Dynamic Delay\Presets",
            TfxParserType = @"TfxAIRDynamicDelay"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRENHANCER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIREnhancer_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Enhancer\Presets",
            TfxParserType = @"TfxAIREnhancer"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRENSEMBLE = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIREnsemble_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Ensemble\Presets",
            TfxParserType = @"TfxAIREnsemble"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRFILTERGATE = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRFilterGate_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Filter Gate\Presets",
            TfxParserType = @"TfxAIRFilterGate"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRFLANGER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRFlanger_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Flanger\Presets",
            TfxParserType = @"TfxAIRFlanger"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRFREQUENCYSHIFTER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRFrequencyShifter_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Frequency Shifter\Presets",
            TfxParserType = @"TfxAIRFrequencyShifter"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRFUZZWAH = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRFuzz-Wah_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Fuzz-Wah\Presets",
            TfxParserType = @"TfxAIRFuzzWah"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRKILLEQ = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRKillEQ_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Kill EQ\Presets",
            TfxParserType = @"TfxAIRKillEQ"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRLOFI = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRLo-Fi_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Lo-Fi\Presets",
            TfxParserType = @"TfxAIRLoFi"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRMAXIMIZER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRMaximizer_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Maximizer\Presets",
            TfxParserType = @"TfxAIRMaximizer"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRMULTI_CHORUS = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRMulti-Chorus_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Multi-Chorus\Presets",
            TfxParserType = @"TfxAIRMulti_Chorus"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRMULTI_DELAY = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRMulti-Delay_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Multi-Delay\Presets",
            TfxParserType = @"TfxAIRMULTI_Delay"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRNONLINEARREVERB = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRNon-LinearReverb_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Non-Linear Reverb\Presets",
            TfxParserType = @"TfxAIRNonLinearReverb"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRPARAMETRICEQ = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRParametricEQ_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR ParametricEQ\Presets",
            TfxParserType = @"TfxAIRParametricEQ"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRPHASER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRPhaser_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Phaser\Presets",
            TfxParserType = @"TfxAIRPhaser"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRPUMPER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRPumper_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Pumper\Presets",
            TfxParserType = @"TfxAIRPumper"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRREVERB = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRReverb_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Reverb\Presets",
            TfxParserType = @"TfxAIRReverb"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRSATURATIONFILTER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRSaturationFilter_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Saturation Filter\Presets",
            TfxParserType = @"TfxAIRSaturationFilter"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRSPECTRAL = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRSpectral_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Spectral\Presets",
            TfxParserType = @"TfxAIRSpectral"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRSPRINGREVERB = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRSpringReverb_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Spring Reverb\Presets",
            TfxParserType = @"TfxAIRSpringReverb"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRSTEREOWIDTH = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRStereoWidth_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Stereo Width\Presets",
            TfxParserType = @"TfxAIRStereoWidth"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRTALKBOX = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRTalkbox_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Talkbox\Presets",
            TfxParserType = @"TfxAIRTalkbox"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRTUBEDRIVE = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRTubeDrive_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR TubeDrive\Presets",
            TfxParserType = @"TfxAIRTubeDrive"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_AIRVINTAGEFILTER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\AIRVintageFilter_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\AIR Vintage Filter\Presets",
            TfxParserType = @"TfxAIRVintageFilter"
        };
        
                // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_BOOM = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Boom_x64.dll",
            PresetDirectory = @"C:\ProgramData\AIR Music Technology\Boom\Presets",
            TfxParserType = @"TfxBoom"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_DB33 = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\DB-33_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\DB-33\Presets",
            TfxParserType = @"TfxDb33"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_DB33FX = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\DB-33FX.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\DB-33FX\Presets",
            TfxParserType = @"TfxDb33Fx"
        };

    

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_LOOM = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Loom_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Loom\Presets",
            TfxParserType = @"TfxLoom"
        };

       

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_THERISER = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\theRiser_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\theRiser\Presets",
            TfxParserType = @"TfxTheRiser"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_VACUUM = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Vacuum_x64.dll",
            PresetDirectory = @"C:\ProgramData\AIR Music Technology\Vacuum\Presets",
            TfxParserType = @"TfxVacuum"
        };

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_VACUUMPRO = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\VacuumPro_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\VacuumPro\Presets",
            TfxParserType = @"TfxVacuumPro"
        };

    

        // ReSharper disable once UnusedMember.Global
        public static readonly AirMusicTechTestSetup TESTSETUP_XPAND2 = new AirMusicTechTestSetup
        {
            PluginDll = @"C:\Program Files\Steinberg\VstPlugins\Xpand!2_x64.dll",
            PresetDirectory = @"C:\Program Files (x86)\AIR Music Technology\Xpand!2\Presets",
            TfxParserType = @"TfxXpand2"
        };


    }

    public class AirMusicTechTestSetup
    {
        public string PluginDll { get; set; }
        public string PresetDirectory { get; set; }
        public string TfxParserType { get; set; }
    }

    public class ChunkAnalysisResult
    {
        public byte[] BlockMagic { get; set; }
        public bool HasPresetNameAtEnd { get; set; }
        public string PluginName { get; set; }
        public byte[] EndChunk { get; set; }
        public bool HasMidi { get; set; }
        public bool HasEmptyMidi { get; set; }
        public int NumParameters { get; set; }
    }

    public class TfxAnalysisResult
        {

            public int NumParameters { get; set; }
        }
    }