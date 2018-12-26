using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Shell32;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Anotar.Catel;
using Catel.Collections;
using Squirrel.Shell;
using File = System.IO.File;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public abstract class u_he : AbstractVendorPresetParser
    {
        private Regex parsingRegex = new Regex(@"^(?<type>.*):(\r\n|\r|\n)'(?<value>.*)'",
        RegexOptions.Multiline | RegexOptions.Compiled);

        public void H2PScanBanks(string dataDirectoryName, string productName, bool userPresets)
        {
            var rootDirectory = GetPresetDirectory(dataDirectoryName, productName, userPresets);
            var directoryInfo = new DirectoryInfo(rootDirectory);

            string bankName = "Factory Bank";
            if (userPresets)
            {
                bankName = "User Bank";
            }

            if (!directoryInfo.Exists)
            {
                return;
            }

            RootBank.PresetBanks.Add(H2PScanBank(bankName, directoryInfo));
            
        }

        public PresetBank H2PScanBank(string name, DirectoryInfo directory)
        {
            PresetBank bank = new PresetBank
            {
                BankName = name
            };

            foreach (var file in directory.EnumerateFiles("*.h2p"))
            {
                Preset preset = new Preset();
                preset.PresetName = file.Name.Replace(".h2p", "");
                preset.SetPlugin(VstPlugin);
                preset.PresetBank = bank;

                var fs = file.OpenRead();

                preset.PresetData = new byte[fs.Length];
                fs.Read(preset.PresetData, 0, (int)fs.Length);
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
            ObservableCollection<ObservableCollection<string>> typeCollection = new ObservableCollection<ObservableCollection<string>>(); 
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
                result.Add(match.Groups["type"].Value, match.Groups["value"].Value);
            }

            return result;
        }

        public string getDataDirectory(string dataDirectoryName)
        {
            var vstPluginsPath = Path.GetDirectoryName(VstPlugin.DllPath);

            return Path.Combine(vstPluginsPath, dataDirectoryName);
        }

        public string GetPresetDirectory(string dataDirectoryName, string productName, bool userPresets)
        {
            object[] args = new object[] { getDataDirectory(dataDirectoryName), productName, userPresets };

            Thread staThread = new Thread(new ParameterizedThreadStart(GetPresetDirectorySTA));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(args);
            staThread.Join();

            return (string)args[0];
        }

        public void GetPresetDirectorySTA(object param)
        {
            object[] args = (object[])param;
            string dataDirectoryName = (string)args[0];
            string productName = (string)args[1];
            bool userPresets = (bool)args[2];

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
                LogTo.Error("Unable to find the data directory, aborting.");
                LogTo.Debug("Estimated shortcut directory name is " + shortCutDataDirectoryName);
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

        public static bool IsShortcut(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
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
                LogTo.Error("Error while trying to resolve the shortcut {0} because of {1} {2}", path, e.Message, e);
                LogTo.Debug(e.StackTrace);
            }
            
            var shell = new Shell();
            var folder = shell.NameSpace(directory);
            var folderItem = folder.ParseName(file);

            return folderItem.IsLink;
        }

        public static string ResolveShortcutSquirrel(string path)
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
                LogTo.Error("Error while trying to resolve the shortcut {0} because of {1} {2}", path, e.Message, e);
                LogTo.Debug(e.StackTrace);
            }

            return null;
        }

        public static string ResolveShortcut(string path)
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