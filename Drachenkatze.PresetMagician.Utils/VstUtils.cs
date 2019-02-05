using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.Utils
{
    public static class VstUtils
    {
        public static string PluginIdNumberToIdString(int pluginUniqueID)
        {
            byte[] fxIdArray = BitConverter.GetBytes(pluginUniqueID);
            Array.Reverse(fxIdArray);
            string fxIdString = BinaryFile.ByteArrayToString(fxIdArray);
            return fxIdString;
        }

        public static int PluginIdStringToIdNumber(string fxIdString)
        {
            byte[] pluginUniqueIDArray = BinaryFile.StringToByteArray(fxIdString); // 58h8 = 946354229
            Array.Reverse(pluginUniqueIDArray);
            int pluginUniqueID = BitConverter.ToInt32(pluginUniqueIDArray, 0);
            return pluginUniqueID;
        }

        public static List<string> EnumeratePlugins(string pluginDirectory)
        {
            var vstPlugins = new List<string>();
            foreach (var file in Directory.EnumerateFiles(
                pluginDirectory, "*.dll", SearchOption.AllDirectories))
            {
                vstPlugins.Add(file);
            }

            return vstPlugins;
        }

        public static string GetDefaultNativeInstrumentsUserContentDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Native Instruments\User Content");
        }

        public static string GetVstWorkerLogDirectory()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"\Drachenkatze\PresetMagician.RemoteVstHost\Logs\");

            Directory.CreateDirectory(directory);

            return directory;
        }

        public static string GetWorkerPluginLog(int pid, Guid guid)
        {
            return Path.Combine(GetVstWorkerLogDirectory(), $"PresetMagician.RemoteVstHost{pid}_{guid}.log");
        }

        public static void CleanupVstWorkerLogDirectory()
        {
            var vstWorkerLogDirectory = GetVstWorkerLogDirectory();
            
            var directory = new DirectoryInfo(vstWorkerLogDirectory);

            try
            {
                foreach (var file in directory.EnumerateFiles("*.*"))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}