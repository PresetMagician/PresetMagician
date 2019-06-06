using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using K4os.Compression.LZ4;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;
using PresetMagician.Core.Services;
using PresetMagician.RemoteVstHost;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Utils;
using PresetMagician.Utils.Logger;
using PresetMagician.VstHost.VST;

namespace PresetMagician.VendorPresetParserTest
{
    internal class Program
    {
        public static Dictionary<int, int> NumBankMissingsOk = new Dictionary<int, int>()
        {
            {1917283917, 30},
            {1836019532, 2},


            {2019642689, 999},
            {1683444545, 999},
            {1750287937, 999},
            {1985106753, 999},
            {1147294785, 999},
            {1749241153, 999},
            {1953711169, 999},
            {1380732481, 999},
            {1752057153, 999},
            {1936606529, 999},
            {1818839873, 999},
            {1853441089, 999},
            {1936216129, 999},
            {2017612865, 999},
            {1816548929, 999},
            {1718570049, 999},
            {1735149121, 999},
            {1750550081, 999},
            {1920356929, 999},
            {1147552833, 999},
            {1950828097, 999},
            {1886212161, 999},
            {1816415553, 999},
            {1668305729, 999},
            {1363497025, 999},
            {1919435585, 999},
            {1886208833, 999},
            {1816548161, 999}
        };

        public static void Main(string[] args)
        {
            FrontendInitializer.RegisterTypes(ServiceLocator.Default);
            FrontendInitializer.Initialize(ServiceLocator.Default);
            var vendorPresetParserService = ServiceLocator.Default.ResolveType<VendorPresetParserService>();
            var logger = new RollingInMemoryLogListener();
            LogManager.AddListener(logger);

            var pluginTestDirectory = @"C:\Program Files\VSTPlugins";
            var testResults = new List<PluginTestResult>();

            var presetParserDictionary = vendorPresetParserService.GetPresetHandlerListByPlugin();


            var testData = ReadTestData();
            var ignoredPlugins = ReadIgnoredPlugins();

            List<string> IgnoredPresetParsers = new List<string>();
            IgnoredPresetParsers.Add("VoidPresetParser");

            var localLogger = new MiniConsoleLogger();
            var hasIgnored = false;
            localLogger.SetConsoleLogLevelFilter(new HashSet<LogLevel> {LogLevel.Error, LogLevel.Warning});

            if (args.Length > 0)
            {
                foreach (var key in presetParserDictionary.Keys.ToList())
                {
                    if (!presetParserDictionary[key].PresetParserType.ToLower().Contains(args[0].ToLower()))
                    {
                        presetParserDictionary.Remove(key);
                        hasIgnored = true;
                    }
                }
            }

            foreach (var presetParserKeyValue in presetParserDictionary)
            {
                var presetParser = presetParserKeyValue.Value;
                var pluginId = presetParserKeyValue.Key;

                if (IgnoredPresetParsers.Contains(presetParser.PresetParserType))
                {
                    continue;
                }

                if (IsIgnored(ignoredPlugins, presetParser.PresetParserType, pluginId))
                {
                    continue;
                }

                Console.Write(presetParser.PresetParserType + ": ");

                var start = DateTime.Now;

                var pluginLocation = new PluginLocation
                {
                    DllPath = @"C:\Program Files\VstPlugins\Foobar.dll", IsPresent = true
                };

                var plugin = new Plugin
                {
                    VstPluginId = pluginId, PluginLocation = pluginLocation,
                    PluginInfo = new VstPluginInfoSurrogate
                        {ProgramCount = 1, Flags = VstPluginFlags.ProgramChunks, PluginID = pluginId}
                };

                var stubProcess = new StubVstHostProcess();
                stubProcess.PluginId = pluginId;

                var remoteInstance = new RemotePluginInstance(stubProcess, plugin);

                presetParser.DataPersistence = new NullPresetPersistence();
                presetParser.PluginInstance = remoteInstance;
                presetParser.RootBank = plugin.RootBank.First();
                presetParser.Logger.Clear();
                presetParser.Logger.MirrorTo(localLogger);

                var testResult = new PluginTestResult
                {
                    VendorPresetParser = presetParser.PresetParserType,
                    PluginId = plugin.VstPluginId
                };

                double timeForNumPresets = 0;
                double timeForDoScan = 0;
                double totalTime = 0;
                try
                {
                    presetParser.Init();
                    testResult.ReportedPresets = presetParser.GetNumPresets();
                    timeForNumPresets = (DateTime.Now - start).TotalSeconds;
                    start = DateTime.Now;
                    presetParser.DoScan().GetAwaiter().GetResult();
                    timeForDoScan = (DateTime.Now - start).TotalSeconds;
                    totalTime = timeForNumPresets + timeForDoScan;
                }
                catch (Exception e)
                {
                    testResult.Error = "Errored";
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                testResult.Presets = plugin.Presets.Count;

                var timePerPreset = (totalTime / testResult.Presets) * 1000;
                // ReSharper disable once LocalizableElement
                Console.WriteLine(
                    $"{testResult.Presets} parsed in {totalTime:F3}s (avg {timePerPreset:F3}ms / Preset, DoScan {timeForDoScan:F3}s, NumPresets {timeForNumPresets:F3}s");

                var testDataEntries = GetTestDataEntries(testData, presetParser.PresetParserType, pluginId);
                var hasTestDataEntries = testDataEntries.Count > 0;
                var testDataOk = true;
                foreach (var preset in plugin.Presets)
                {
                    if (preset.Metadata.BankPath == "")
                    {
                        testResult.BankMissing++;
                    }

                    foreach (var testDataEntry in testDataEntries.ToList())
                    {
                        if (preset.Metadata.PresetName == testDataEntry.ProgramName &&
                            preset.Metadata.BankPath == testDataEntry.BankPath)
                        {
                            var testFilename = PathUtils.SanitizeFilename(
                                testDataEntry.PresetParser + "." + preset.OriginalMetadata.PresetName +
                                ".testdata");
                            var myDocumentsTestDataFile = Path.Combine(GetPatchFilesDirectory(), testFilename);
                            var localTestDataFile = Path.Combine("TestData", testFilename);


                            var presetHash = testDataEntry.Hash.TrimEnd();
                            if (preset.PresetHash != presetHash)
                            {
                                var fileMessage = "";
                                var wrongPresetData = myDocumentsTestDataFile + ".wrong";
                                testDataOk = false;

                                if (File.Exists(myDocumentsTestDataFile))
                                {
                                    fileMessage = $"Original preset data in {myDocumentsTestDataFile}" +
                                                  Environment.NewLine +
                                                  $"Current (wrong) preset data in {wrongPresetData}";
                                }
                                else
                                {
                                    fileMessage =
                                        $"Original preset data not found (expected in {myDocumentsTestDataFile})" +
                                        Environment.NewLine +
                                        $"Current (wrong) preset data in {wrongPresetData}";
                                }

                                File.WriteAllBytes(wrongPresetData, LZ4Pickler.Unpickle(
                                    NullPresetPersistence.PresetData[preset.OriginalMetadata.SourceFile]));
                                testResult.DetailedErrors.Add(
                                    $"Found preset {testDataEntry.ProgramName} with bank path " +
                                    $"{testDataEntry.BankPath} but the preset hashes were different. " +
                                    $"Expected hash {presetHash} but found hash {preset.PresetHash}" +
                                    Environment.NewLine + Environment.NewLine + $"{fileMessage}");
                            }
                            else
                            {
                                // Check if the file exists in the output directory
                                if (!File.Exists(myDocumentsTestDataFile))
                                {
                                    if (File.Exists(localTestDataFile))
                                    {
                                        File.Copy(localTestDataFile, myDocumentsTestDataFile);
                                    }
                                    else
                                    {
                                        File.WriteAllBytes(myDocumentsTestDataFile,
                                            LZ4Pickler.Unpickle(
                                                NullPresetPersistence.PresetData[preset.OriginalMetadata.SourceFile]));
                                    }
                                }
                                else
                                {
                                    if (!File.Exists(localTestDataFile))
                                    {
                                        testResult.DetailedErrors.Add(
                                            $"Warning: The preset data file {testFilename} exists in the documents " +
                                            "folder but not in the source folder. Copy from documents to git folder. " +
                                            "If already done, remember to clean the presetparsertest project.");
                                    }
                                }

                                var hash = HashUtils.getIxxHash(File.ReadAllBytes(myDocumentsTestDataFile));

                                if (hash != presetHash)
                                {
                                    testResult.DetailedErrors.Add(
                                        $"Warning: The preset data file {myDocumentsTestDataFile} exists but does not match the " +
                                        $"preset hash from the reference presets. Expected: {testDataEntry.Hash} found {hash}");
                                }
                            }

                            testDataEntries.Remove(testDataEntry);
                        }
                    }
                }

                if (testDataEntries.Count > 0)
                {
                    foreach (var missingTestDataEntry in testDataEntries)
                    {
                        var presetHash = missingTestDataEntry.Hash.TrimEnd();
                        testResult.DetailedErrors.Add(
                            $"Did not find preset {missingTestDataEntry.ProgramName} with bank path " +
                            $"{missingTestDataEntry.BankPath} and hash {presetHash}");
                    }

                    testResult.IsOK = false;
                }

                if (plugin.Presets.Count > 0)
                {
                    var randomPreset = plugin.Presets.OrderBy(qu => Guid.NewGuid()).First();
                    testResult.RndHash = randomPreset.PresetHash;
                    testResult.RndPresetName = randomPreset.Metadata.PresetName;
                    testResult.RndBankPath = randomPreset.Metadata.BankPath;
                }

                var mockFxp = Path.Combine(Directory.GetCurrentDirectory(), "mock.fxp");
                var fxp = new FXP();
                fxp.ReadFile(Path.Combine(Directory.GetCurrentDirectory(), "test.fxp"));
                fxp.FxID = VstUtils.PluginIdNumberToIdString(pluginId);
                fxp.WriteFile(mockFxp);
                // Test additional banks
                var bankFile = new BankFile();
                bankFile.Path = mockFxp;
                bankFile.BankName = "Default";

                plugin.AdditionalBankFiles.Clear();
                plugin.AdditionalBankFiles.Add(bankFile);

                bool additionalBankFileCountOk = false;


                if (presetParser.GetNumPresets() == testResult.ReportedPresets + 1)
                {
                    additionalBankFileCountOk = true;
                }
                else
                {
                    testResult.Error += " additionalBankFileCount failed";
                }

                plugin.Presets.Clear();
                NullPresetPersistence.PresetData.Clear();
                presetParser.DoScan().GetAwaiter().GetResult();

                var additionalBankFileScanOk = false;

                if (plugin.Presets.Count == testResult.Presets + 1)
                {
                    additionalBankFileScanOk = true;
                }
                else
                {
                    testResult.Error += " additionalBankFileScan failed";
                }

                bool bankMissingOk = false;
                if (NumBankMissingsOk.ContainsKey(testResult.PluginId))
                {
                    if (testResult.BankMissing <= NumBankMissingsOk[testResult.PluginId])
                    {
                        bankMissingOk = true;
                    }
                }
                else
                {
                    if (testResult.BankMissing < 2)
                    {
                        bankMissingOk = true;
                    }
                }

                if (hasTestDataEntries && testDataOk && testResult.Presets > 5 && bankMissingOk &&
                    testResult.Presets == testResult.ReportedPresets && additionalBankFileCountOk &&
                    additionalBankFileScanOk)
                {
                    testResult.IsOK = true;
                }

                testResults.Add(testResult);

                NullPresetPersistence.PresetData.Clear();
            }


            var consoleTable = ConsoleTable.From(from testRes in testResults
                where testRes.IsOK == false
                orderby testRes.Presets
                select testRes);

            Console.WriteLine(consoleTable.ToMinimalString());

            foreach (var testRes in (from testRes in testResults
                where testRes.DetailedErrors.Count > 0
                orderby testRes.Presets
                select testRes))
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Detailed Errors for {testRes.VendorPresetParser}");
                Console.WriteLine($"------------------------------------------------------------");

                foreach (var detailedError in testRes.DetailedErrors)
                {
                    Console.WriteLine($"Error #{testRes.DetailedErrors.IndexOf(detailedError)}: {detailedError}");
                }
            }

            Console.WriteLine($"Stuff left: {consoleTable.Rows.Count} / {presetParserDictionary.Count}");

            foreach (var data in GlobalMethodTimeLogger.GetTopMethods())
            {
                Console.WriteLine($"{data.Name}: {data.Duration.TotalSeconds.ToString()}ms");
            }

            if (hasIgnored)
            {
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
                Console.WriteLine("Warning: Filter active!!");
            }
        }

        public static string GetPatchFilesDirectory()
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $@"PresetMagician\VendorPresetParserTest\");

            Directory.CreateDirectory(directory);
            return directory;
        }

        public static List<TestData> ReadTestData()
        {
            var config = new Configuration(CultureInfo.InvariantCulture);
            config.MissingFieldFound = null;
            using (var reader = new StreamReader("testdata.csv"))


            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<TestData>().ToList();
            }
        }

        public static List<IgnoredPlugins> ReadIgnoredPlugins()
        {
            using (var reader = new StreamReader("ignoredplugins.csv"))
            using (var csv = new CsvReader(reader))
            {
                return csv.GetRecords<IgnoredPlugins>().ToList();
            }
        }

        public static List<TestData> GetTestDataEntries(List<TestData> testData, string presetParser, int pluginId)
        {
            return (from testDataEntry in testData
                where
                    testDataEntry.PluginId == pluginId && testDataEntry.PresetParser == presetParser
                select testDataEntry).ToList();
        }

        public static bool IsIgnored(List<IgnoredPlugins> ignoredPlugins, string presetParser, int pluginId)
        {
            return (from testDataEntry in ignoredPlugins
                where
                    testDataEntry.PluginId == pluginId && testDataEntry.PresetParser == presetParser
                select testDataEntry).Any();
        }
    }

    public class StubVstHostProcess : VstHostProcess
    {
        public int PluginId { get; set; }

        public override IRemoteVstService GetVstService()
        {
            return new StubRemoteVstService(PluginId);
        }
    }


    public class StubRemoteVstService : IRemoteVstService
    {
        private byte[] _bankData = Encoding.UTF8.GetBytes("ClearDirtyFlag");
        private byte[] _presetData = Encoding.UTF8.GetBytes("ClearDirtyFlag");
        private int _pluginId;

        public StubRemoteVstService(int pluginId)
        {
            _pluginId = pluginId;
        }

        public void PatchPluginToMidiInput(Guid guid, MidiInputDevice device)
        {
        }

        public void UnpatchPluginFromMidiInput()
        {
        }

        public void PatchPluginToAudioOutput(Guid guid, AudioOutputDevice device, int latency)
        {
        }

        public void UnpatchPluginFromAudioOutput()
        {
        }

        public float GetParameter(Guid pluginGuid, int parameterIndex)
        {
            return 0;
        }

        public void KillSelf()
        {
        }

        public DateTime GetLastModifiedDate(string file)
        {
            return DateTime.Now;
        }

        public bool Exists(string file)
        {
            return true;
        }

        public long GetSize(string file)
        {
            return 0;
        }

        public string GetHash(string file)
        {
            return "";
        }

        public byte[] GetContents(string file)
        {
            return null;
        }

        public int GetPluginVendorVersion(Guid pluginGuid)
        {
            return 0;
        }

        public string GetPluginProductString(Guid pluginGuid)
        {
            return null;
        }

        public string GetEffectivePluginName(Guid pluginGuid)
        {
            return null;
        }

        public bool Ping()
        {
            return true;
        }

        public Guid RegisterPlugin(string dllPath, bool backgroundProcessing = true)
        {
            var guid = Guid.NewGuid();
            return guid;
        }

        public void UnregisterPlugin(Guid guid)
        {
        }

        public string GetPluginHash(Guid guid)
        {
            return null;
        }

        public void LoadPlugin(Guid guid, bool debug = false)
        {
        }

        public void UnloadPlugin(Guid guid)
        {
        }

        public void ReloadPlugin(Guid guid)
        {
        }

        private RemoteVstPlugin GetPluginByGuid(Guid guid)
        {
            return null;
        }

        public bool OpenEditorHidden(Guid pluginGuid)
        {
            return true;
        }

        public bool OpenEditor(Guid pluginGuid, bool topmost = true)
        {
            return true;
        }

        public void CloseEditor(Guid pluginGuid)
        {
        }

        public byte[] CreateScreenshot(Guid pluginGuid)
        {
            return null;
        }

        public string GetPluginName(Guid pluginGuid)
        {
            return null;
        }

        public string GetPluginVendor(Guid pluginGuid)
        {
            return null;
        }

        public List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid)
        {
            return new List<PluginInfoItem>();
        }

        public VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid)
        {
            var info = new VstPluginInfoSurrogate
                {ProgramCount = 1, Flags = VstPluginFlags.ProgramChunks, PluginID = _pluginId};
            return info;
        }

        public void SetProgram(Guid pluginGuid, int program)
        {
        }

        public string GetCurrentProgramName(Guid pluginGuid)
        {
            return "nope";
        }

        public byte[] GetChunk(Guid pluginGuid, bool isPreset)
        {
            if (isPreset)
            {
                return _presetData;
            }
            else
            {
                return _bankData;
            }
        }

        public void SetChunk(Guid pluginGuid, byte[] data, bool isPreset)
        {
            if (isPreset)
            {
                _presetData = data;
            }
            else
            {
                _bankData = data;
            }
        }

        public void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData,
            int initialDelay)
        {
        }

        public void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData)
        {
        }
    }

    internal class NullPresetPersistence : IDataPersistence
    {
        public static Dictionary<string, byte[]> PresetData = new Dictionary<string, byte[]>();

        public event EventHandler<PresetUpdatedEventArgs> PresetUpdated;
#pragma warning disable 1998
        public async Task PersistPreset(PresetParserMetadata presetMetadata, byte[] data, bool force = false)
#pragma warning restore 1998
        {
            var preset = new Preset();
            preset.Plugin = presetMetadata.Plugin;
            preset.Plugin.Presets.Add(preset);

            preset.SetFromPresetParser(presetMetadata);
            preset.PresetHash = HashUtils.getIxxHash(data);
            preset.PresetSize = data.Length;
            preset.PresetCompressedSize = data.Length;

            try
            {
                PresetData.Add(preset.OriginalMetadata.SourceFile, LZ4Pickler.Pickle(data));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to add {preset.OriginalMetadata.SourceFile}");
                throw e;
            }
        }
    }

    class PluginTestResult
    {
        public int Presets { get; set; }
        public int ReportedPresets { get; set; }
        public string VendorPresetParser { get; set; }
        public string Error { get; set; }
        public int PluginId { get; set; }
        public int BankMissing { get; set; }
        public string RndPresetName { get; set; }
        public string RndBankPath { get; set; }
        public string RndHash { get; set; }
        public List<string> DetailedErrors = new List<string>();

        public string CsvData
        {
            get
            {
                bool bankMissingOk = false;
                if (Program.NumBankMissingsOk.ContainsKey(PluginId))
                {
                    if (BankMissing <= Program.NumBankMissingsOk[PluginId])
                    {
                        bankMissingOk = true;
                    }
                }
                else
                {
                    if (BankMissing < 2)
                    {
                        bankMissingOk = true;
                    }
                }

                if (Presets > 5 && bankMissingOk && Presets == ReportedPresets)
                {
                    return string.Join(",", VendorPresetParser, PluginId.ToString(), RndPresetName, RndBankPath,
                        RndHash, DateTime.Now.ToString());
                }

                return "";
            }
        }

        public bool IsOK { get; set; }
        public string RandomPresetSource { get; set; }
    }

    public class TestData
    {
        [Name("PresetParser")] public string PresetParser { get; set; }

        [Name("PluginId")] public int PluginId { get; set; }

        [Name("ProgramName")] public string ProgramName { get; set; }

        [Name("BankPath")] public string BankPath { get; set; }

        [Name("Hash")] public string Hash { get; set; }

        [Name("LastUpdated")] [Optional] public string LastUpdated { get; set; }
    }

    public class IgnoredPlugins
    {
        [Name("PresetParser")] public string PresetParser { get; set; }

        [Name("PluginId")] public int PluginId { get; set; }
    }
}