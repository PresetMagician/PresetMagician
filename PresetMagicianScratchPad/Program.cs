using System;
using System.Diagnostics;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Properties;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagicianScratchPad
{
    public class MyCustomScriptLoader : ScriptLoaderBase
    {
        public override object LoadFile(string file, Table globalContext)
        {
            if (file == "serpent_module.lua")
            {
                return VendorResources.Lua_Serpent;
            }

            return "";
        }

        public override bool ScriptFileExists(string name)
        {
            if (name == "serpent_module.lua")
            {
                return true;
            }

            return false;
        }
    }

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var file =
                //@"C:\Program Files\Applied Acoustics Systems\String Studio VS-3\Factory Library\Factory Library.VS-3 Pack";
                @"C:\Users\Drachenkatze\AppData\Roaming\Applied Acoustics Systems\Lounge Lizard Session 4\Banks\Lounge Lizard Session.LLS4 Bank";

            var fileData = File.ReadAllText(file);
            //fileData = fileData.Replace("\n", "");
            //fileData = fileData.Replace("\r", "");


            Script script = new Script();
            script.DebuggerEnabled = false;
            script.Options.ScriptLoader = new MyCustomScriptLoader()
            {
                ModulePaths = new string[] {"?_module.lua"}
            };

            script.DoString(VendorResources.AppliedAcousticSystems_LibraryParser);


            var func = script.Globals.Get("loadLibrary");

            var sw = new Stopwatch();
            sw.Start();
            var result = script.Call(func, fileData, "Factory", "Factory Library", file);

            var p = new PresetParserMetadata();
            foreach (var table in result.Table.Values)
            {
                File.WriteAllText(@"C:\Users\Drachenkatze\Documents\PresetMagician\test.dat",
                    (string) table.Table["presetData"]);
                Console.WriteLine(table.Table["presetName"]);
                Console.WriteLine(table.Table["rawMetaData"]);

                var metadata = (Table) table.Table["metaData"];

                if (metadata != null)
                {
                    Console.WriteLine(metadata["modes"].GetType().FullName);

                    if (metadata["creator"] != null)
                    {
                        p.Author = (string) metadata["creator"];
                    }

                    if (metadata["comment"] != null)
                    {
                        p.Comment = (string) metadata["comment"];
                    }

                    var modes = (Table) metadata["modes"];

                    if (modes != null)
                    {
                        foreach (var mode in modes.Values)
                        {
                            p.Characteristics.Add(new Characteristic() {CharacteristicName = mode.String});
                            Console.WriteLine(mode.String);
                        }
                    }

                    var categories = (Table) metadata["categories"];

                    if (categories != null)
                    {
                        foreach (var category in categories.Values)
                        {
                            var splittedString = category.String.Split('.');

                            if (splittedString.Length == 2)
                            {
                                p.Types.Add(new Type() {TypeName = splittedString[0], SubTypeName = splittedString[1]});
                            }

                            if (splittedString.Length == 1)
                            {
                                p.Types.Add(new Type()
                                {
                                    TypeName = splittedString[0]
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}