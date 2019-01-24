using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services;
using PresetMagician.Collections;
using SharedModels;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace PresetMagician.VendorPresetParserTest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var pluginTestDirectory = @"C:\Program Files\VSTPlugins";
            var testResults = new List<PluginTestResult>();

            var plugins = VstUtils.EnumeratePlugins(pluginTestDirectory);
            var presetParserDictionary = VendorPresetParser.GetPresetHandlerList();

            ApplicationDatabaseContext.OverrideDbPath = Path.Combine(Directory.GetCurrentDirectory(), "test.sqlite3");

            if (File.Exists(ApplicationDatabaseContext.OverrideDbPath))
            {
                File.Delete(ApplicationDatabaseContext.OverrideDbPath);
            }

            Console.WriteLine(ApplicationDatabaseContext.OverrideDbPath);
            var databaseService = new DatabaseService();
            var vstService = new VstService(databaseService);
            var testData = ReadTestData();

            Console.WriteLine(testData);

            List<string> IgnoredPresetParsers = new List<string>();
            IgnoredPresetParsers.Add("VoidPresetParser");

            foreach (var presetParserKeyValue in presetParserDictionary)
            {
                var presetParser = presetParserKeyValue.Value;
                var pluginId = presetParserKeyValue.Key;

                if (IgnoredPresetParsers.Contains(presetParser.PresetParserType))
                {
                    continue;
                }

                Console.WriteLine(presetParser.PresetParserType);

                var plugin = new Plugin {PluginId = pluginId};
                presetParser.PresetDataStorer = new NullPresetStorer();
                presetParser.Plugin = plugin;
                presetParser.RootBank = plugin.RootBank.First();
                presetParser.Presets = new ObservableCollection<Preset>();

                var testResult = new PluginTestResult
                {
                    VendorPresetParser = presetParser.PresetParserType,
                    PluginId = plugin.PluginId
                };

                try
                {
                    presetParser.DoScan();
                }
                catch (Exception e)
                {
                    testResult.Error = e.Message;
                }

                testResult.Presets = plugin.Presets.Count;

                var testDataEntry = GetTestDataEntry(testData, presetParser.PresetParserType, pluginId);
                var foundPreset = false;
                foreach (var preset in plugin.Presets)
                {
                    if (preset.BankPath == "")
                    {
                        testResult.BankMissing++;
                    }

                    if (testDataEntry != null)
                    {
                        if (preset.PresetName == testDataEntry.ProgramName &&
                            preset.PresetHash == testDataEntry.Hash &&
                            preset.BankPath == testDataEntry.BankPath)
                        {
                            foundPreset = true;
                        }
                    }
                }

                if (plugin.Presets.Count > 0)
                {
                    var randomPreset = plugin.Presets.OrderBy(qu => Guid.NewGuid()).First();
                    testResult.RndHash = randomPreset.PresetHash;
                    testResult.RndPresetName = randomPreset.PresetName;
                    testResult.RndBankPath = randomPreset.BankPath;
                    testResult.RandomPresetSource = randomPreset.SourceFile;
                }

                if (foundPreset && plugin.Presets.Count > 5 && testResult.BankMissing < 2)
                {
                    testResult.IsOK = true;
                }


                testResults.Add(testResult);
            }
            /*foreach (var pluginDll in plugins)
            {
                try
                {
                    
                    Console.WriteLine(pluginDll);

                    var remoteVstService = vstService.LoadVstInteractive(plugin).Result;
                    VendorPresetParser.DeterminatePresetParser(plugin, remoteVstService);

                    if (!plugin.PresetParser.GetSupportedPlugins().Contains(plugin.PluginId))
                    {
                        vstService.UnloadVst(plugin).Wait();
                        continue;
                    }
                    plugin.PresetParser.PresetDataStorer = databaseService.GetPresetDataStorer();
                    plugin.PresetParser.Presets = plugin.Presets;
                    plugin.PresetParser.RootBank = plugin.RootBank.First();
                    plugin.PresetParser.RemoteVstService = remoteVstService;

                    plugin.PresetParser.DoScan().Wait();
                    databaseService.GetPresetDataStorer().Flush().Wait();

                    var testResult = new PluginTestResult();
                    testResult.VendorPresetParser = plugin.PresetParser;
                    testResult.PresetCount = plugin.Presets.Count;
                    testResult.DllFilename = plugin.DllFilename;
                    testResult.PluginId = plugin.PluginId;
                    foreach (var preset in plugin.Presets)
                    {
                        if (preset.BankPath == "")
                        {
                            testResult.PresetsWithoutBankPath++;
                        }
                    }

                    foreach (var i in plugin.PresetParser.GetSupportedPlugins())
                    {
                        if (presetParserDictionary.ContainsKey(i))
                        {
                            testResults.Add(testResult);
                            presetParserDictionary.Remove(i);
                        }
                    }
                    vstService.UnloadVst(plugin).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }*/

            var consoleTable = ConsoleTable.From(from testRes in testResults
                where testRes.IsOK == false
                orderby testRes.Presets ascending 
                select testRes);

            Console.WriteLine(consoleTable.ToMinimalString());
        }

        public static IEnumerable<TestData> ReadTestData()
        {
            using (var reader = new StreamReader("testdata.csv"))
            using (var csv = new CsvReader(reader))
            {
                return csv.GetRecords<TestData>().ToList();
            }
        }

        public static TestData GetTestDataEntry(IEnumerable<TestData> testData, string presetParser, int pluginId)
        {
            return (from testDataEntry in testData
                where
                    testDataEntry.PluginId == pluginId && testDataEntry.PresetParser == presetParser
                select testDataEntry).FirstOrDefault();
        }
    }


    class NullPresetStorer : IPresetDataStorer
    {
        public async Task PersistPreset(Preset preset, byte[] data)
        {
            preset.Plugin.Presets.Add(preset);
            preset.PresetHash = HashUtils.getIxxHash(data);
        }

        public async Task Flush()
        {
        }
    }

    class PluginTestResult
    {
        public int Presets { get; set; }
        public string VendorPresetParser { get; set; }
        public string Error { get; set; }
        public int PluginId { get; set; }
        public int BankMissing { get; set; }
        public string RndPresetName { get; set; }
        public string RndBankPath { get; set; }
        public string RndHash { get; set; }

        public string CsvData
        {
            get
            {
                if (Presets > 5 && BankMissing < 2)
                {
                    return string.Join(",", new string[]
                    {
                        VendorPresetParser,
                        PluginId.ToString(),
                        RndPresetName,
                        RndBankPath,
                        RndHash
                    });
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
    }
}