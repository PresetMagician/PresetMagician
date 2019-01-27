using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Catel.Collections;
using Catel.Logging;
using SharedModels;
using Squirrel.Shell;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public abstract class u_he : AbstractVendorPresetParser
    {
        private readonly Regex parsingRegex = new Regex(@"^(?<type>.*):(\r\n|\r|\n)'(?<value>.*)'",
            RegexOptions.Multiline | RegexOptions.Compiled);

        protected void H2PScanBanks(string dataDirectoryName, string productName, bool userPresets)
        {
            PluginInstance.Plugin.Logger.Debug(
                $"Begin H2PScanBanks with dataDirectoryName {dataDirectoryName} product name {productName} and userPresets {userPresets}");
            var rootDirectory = GetPresetDirectory(dataDirectoryName, productName, userPresets);
            PluginInstance.Plugin.Logger.Debug($"Parsing PresetDirectory {rootDirectory}");

            var directoryInfo = new DirectoryInfo(rootDirectory);

            var bankName = "Factory Bank";
            if (userPresets)
            {
                bankName = "User Bank";
            }

            if (!directoryInfo.Exists)
            {
                return;
            }

            RootBank.PresetBanks.Add(H2PScanBank(bankName, directoryInfo));
            PluginInstance.Plugin.Logger.Debug($"End H2PScanBanks");
        }

        private PresetBank H2PScanBank(string name, DirectoryInfo directory)
        {
            PresetBank bank = new PresetBank
            {
                BankName = name
            };

            foreach (var file in directory.EnumerateFiles("*.h2p"))
            {
                PluginInstance.Plugin.Logger.Debug($"Parsing file {file.FullName}");
                Preset preset = new Preset
                {
                    PresetName = file.Name.Replace(".h2p", ""), Plugin = PluginInstance.Plugin, PresetBank = bank
                };

                var fs = file.OpenRead();

                preset.PresetData = new byte[fs.Length];
                fs.Read(preset.PresetData, 0, (int) fs.Length);
                fs.Close();

                var metadata = ExtractMetadata(Encoding.UTF8.GetString(preset.PresetData));

                if (metadata.ContainsKey("Author"))
                {
                    preset.Author = metadata["Author"];
                }

                List<string> comments = new List<string>();

                if (metadata.ContainsKey("Description") && metadata["Description"].Length > 0)
                {
                    comments.Add(metadata["Description"]);
                }

                if (metadata.ContainsKey("Usage") && metadata["Usage"].Length > 0)
                {
                    comments.Add(metadata["Usage"]);
                }

                preset.Comment = string.Join(Environment.NewLine, comments);

                if (metadata.ContainsKey("Categories") && metadata["Categories"].Length > 0)
                {
                    preset.Types = ExtractTypes(metadata["Categories"]);
                }

                if (metadata.ContainsKey("Features") && metadata["Features"].Length > 0)
                {
                    ExtractModes(preset.Modes, metadata["Features"]);
                }

                if (metadata.ContainsKey("Character") && metadata["Character"].Length > 0)
                {
                    ExtractModes(preset.Modes, metadata["Character"]);
                }

                Presets.Add(preset);
            }

            foreach (var subDirectory in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                bank.PresetBanks.Add(H2PScanBank(subDirectory.Name, subDirectory));
            }

            return bank;
        }

        public ObservableCollection<ObservableCollection<string>> ExtractTypes(string types)
        {
            ObservableCollection<ObservableCollection<string>> typeCollection =
                new ObservableCollection<ObservableCollection<string>>();
            var splitTypes = types.Split(',');

            foreach (var splitType in splitTypes)
            {
                var coll = new ObservableCollection<string>();

                coll.AddRange(splitType.Trim().Split(':'));

                typeCollection.Add(coll);
            }

            return typeCollection;
        }

        public void ExtractModes(ObservableCollection<string> modeCollection, string modes)
        {
            var splitModes = modes.Split(',');

            foreach (var splitMode in splitModes)
            {
                modeCollection.Add(splitMode.Trim());
            }
        }

        public Dictionary<string, string> ExtractMetadata(string presetData)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();


            foreach (Match match in parsingRegex.Matches(presetData))
            {
                var type = match.Groups["type"].Value;
                var value = match.Groups["value"].Value;
                try
                {
                    result.Add(type, value);
                }
                catch (ArgumentException)
                {
                    PluginInstance.Plugin.Logger.Debug(
                        $"Unable to add metadata for type {type} with value {value} because {type} already exists.");
                }
            }

            return result;
        }

        public string getDataDirectory(string dataDirectoryName)
        {
            var vstPluginsPath = Path.GetDirectoryName(PluginInstance.Plugin.DllPath);

            return Path.Combine(vstPluginsPath, dataDirectoryName);
        }

        public string GetPresetDirectory(string dataDirectoryName, string productName, bool userPresets)
        {
            object[] args = {getDataDirectory(dataDirectoryName), productName, userPresets};

            Thread staThread = new Thread(GetPresetDirectorySTA);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(args);
            staThread.Join();

            return (string) args[0];
        }

        public void GetPresetDirectorySTA(object param)
        {
            object[] args = (object[]) param;
            string dataDirectoryName = (string) args[0];
            string productName = (string) args[1];
            bool userPresets = (bool) args[2];

            var shortCutDataDirectoryName = getDataDirectory(dataDirectoryName + ".lnk");

            string dataDirectory;

            if (IsShortcut(shortCutDataDirectoryName))
            {
                dataDirectory = ResolveShortcut(getDataDirectory(shortCutDataDirectoryName));
            }
            else
            {
                dataDirectory = getDataDirectory(dataDirectoryName);
            }

            if (dataDirectory == null)
            {
                PluginInstance.Plugin.Logger.Error("Unable to find the data directory, aborting.");
                PluginInstance.Plugin.Logger.Debug("Estimated shortcut directory name is " + shortCutDataDirectoryName);
                return;
            }

            if (userPresets)
            {
                args[0] = Path.Combine(dataDirectory, "UserPresets", productName);
            }
            else
            {
                args[0] = Path.Combine(dataDirectory, "Presets", productName);
            }
        }

        public bool IsShortcut(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            ShellLink shellLink;
            try
            {
                shellLink = new ShellLink(path);

                if (shellLink.Target.Length > 0 && Directory.Exists(shellLink.Target))
                {
                    shellLink.Dispose();
                    return true;
                }

                shellLink.Dispose();
            }
            catch (IOException e)
            {
                PluginInstance.Plugin.Logger.Error("Error while trying to resolve the shortcut {0} because of {1} {2}",
                    path, e.Message, e);
                PluginInstance.Plugin.Logger.Debug(e.StackTrace);
            }

            return false;
        }

        public string ResolveShortcutSquirrel(string path)
        {
            ShellLink shellLink;

            try
            {
                shellLink = new ShellLink(path);

                if (shellLink.Target.Length > 0 && Directory.Exists(shellLink.Target))
                {
                    return shellLink.Target;
                }

                shellLink.Dispose();
            }
            catch (IOException e)
            {
                PluginInstance.Plugin.Logger.Error("Error while trying to resolve the shortcut {0} because of {1} {2}",
                    path, e.Message, e);
                PluginInstance.Plugin.Logger.Debug(e.StackTrace);
            }

            return null;
        }

        public string ResolveShortcut(string path)
        {
            string targetPath;


            if (!IsShortcut(path))
            {
                return string.Empty;
            }

            targetPath = ResolveShortcutSquirrel(path);

            if (targetPath != null)
            {
                return targetPath;
            }

            return null;
        }
    }
}