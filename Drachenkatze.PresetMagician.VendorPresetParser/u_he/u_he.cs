using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shell32;
using System.IO;
using System.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public abstract class u_he : AbstractVendorPresetParser
    {
        public void H2PScanBanks(string dataDirectoryName, string productName, bool userPresets)
        {
            if (Banks == null)
            {
                Banks = new List<PresetBank>();
            }

            var presetDirectory = GetPresetDirectory(dataDirectoryName, productName, userPresets);
            var directoryInfo = new DirectoryInfo(presetDirectory);

            string bankName = "Factory Bank";
            if (userPresets)
            {
                bankName = "User Bank";
            }

            if (!directoryInfo.Exists)
            {
                return;
            }

            Banks.Add(H2PScanBank(bankName, directoryInfo));
            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                Banks.Add(H2PScanBank(directory.Name, directory));
            }
        }

        public PresetBank H2PScanBank(string name, DirectoryInfo directory)
        {
            PresetBank bank = new PresetBank
            {
                BankName = name
            };

            foreach (var file in directory.EnumerateFiles())
            {
                VSTPreset vstPreset = new VSTPreset();
                vstPreset.PresetName = file.Name.Replace(".h2p", "");
                vstPreset.SetPlugin(VstPlugin);
                vstPreset.BankName = bank.BankName;

                var fs = file.OpenRead();

                vstPreset.PresetData = new byte[fs.Length];
                fs.Read(vstPreset.PresetData, 0, (int)fs.Length);
                fs.Close();

                bank.VSTPresets.Add(vstPreset);
            }

            return bank;
        }

        public string getDataDirectory(string dataDirectoryName)
        {
            var vstPluginsPath = Path.GetDirectoryName(VstPlugin.PluginDLLPath);

            return Path.Combine(vstPluginsPath, dataDirectoryName);
        }

        public string GetPresetDirectory(string dataDirectoryName, string productName, bool userPresets)
        {
            object[] args = new object[] { getDataDirectory(dataDirectoryName), productName, userPresets };

            Thread staThread = new Thread(new ParameterizedThreadStart(GetPresetDirectorySTA));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(args);
            staThread.Join();

            Debug.WriteLine(args[0]);
            return (string)args[0];
        }

        public void GetPresetDirectorySTA(object param)
        {
            object[] args = (object[])param;
            string dataDirectoryName = (string)args[0];
            string productName = (string)args[1];
            bool userPresets = (bool)args[2];

            var shortCutDataDirectoryName = getDataDirectory(dataDirectoryName + ".lnk");

            Debug.WriteLine("Estimated shortcut directory name is " + shortCutDataDirectoryName);
            string dataDirectory;

            if (IsShortcut(shortCutDataDirectoryName))
            {
                dataDirectory = ResolveShortcut(getDataDirectory(shortCutDataDirectoryName));
            }
            else
            {
                dataDirectory = getDataDirectory(dataDirectoryName);
            }

            Debug.WriteLine("Data directory is " + dataDirectory);

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

            var shell = new Shell();
            var folder = shell.NameSpace(directory);
            var folderItem = folder.ParseName(file);

            return folderItem.IsLink;
        }

        public static string ResolveShortcut(string path)
        {
            var directory = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            if (!IsShortcut(path))
            {
                return string.Empty;
            }

            var shell = new Shell();
            var folder = shell.NameSpace(directory);
            var folderItem = folder.ParseName(file);
            if (folderItem == null)
            {
                return string.Empty;
            }

            string targetPath;

            try
            {
                var link = (ShellLinkObject)folderItem.GetLink;
                targetPath = link.Target.Path;
            }
            catch (System.UnauthorizedAccessException)
            {
                WshShell wshShell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(path);
                targetPath = shortcut.TargetPath;
            }

            return targetPath;
        }
    }
}