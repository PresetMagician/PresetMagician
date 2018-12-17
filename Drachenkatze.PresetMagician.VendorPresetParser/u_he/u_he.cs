using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shell32;
using System.IO;
using System.Threading;
using Squirrel.Shell;
using File = System.IO.File;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public abstract class u_he : AbstractVendorPresetParser
    {
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

            foreach (var file in directory.EnumerateFiles())
            {
                Preset preset = new Preset();
                preset.PresetName = file.Name.Replace(".h2p", "");
                preset.SetPlugin(VstPlugin);
                preset.PresetBank = bank;

                var fs = file.OpenRead();

                preset.PresetData = new byte[fs.Length];
                fs.Read(preset.PresetData, 0, (int)fs.Length);
                fs.Close();

                Presets.Add(preset);

            }

            foreach (var subDirectory in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                bank.PresetBanks.Add(H2PScanBank(subDirectory.Name, subDirectory));
            }

            return bank;
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

            if (dataDirectory == null)
            {
                #warning Implement error handler here plus logging
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
            catch (System.IO.IOException e)
            {
                // TODO: Implement catel logger
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
            catch (System.IO.IOException e)
            {
                // TODO: Implement catel logger
            }

            return null;
        }

        public static string ResolveShortcutShell32(string path)
        {
            var directory = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            var shell = new Shell();
            var folder = shell.NameSpace(directory);
            var folderItem = folder.ParseName(file);
            if (folderItem == null)
            {
                return null;
            }


            try
            {
                var link = (ShellLinkObject) folderItem.GetLink;
                return link.Target.Path;
            }
            catch (Exception e)
            {
                return null;
            }
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
            
            /*targetPath = ResolveShortcutShell32(path);

            if (targetPath != null)
            {
                return targetPath;
            }*/

            return null;
        }
    }
}