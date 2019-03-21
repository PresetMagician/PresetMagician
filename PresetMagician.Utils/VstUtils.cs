using System;
using System.IO;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Plugin;

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

        public static string GetDefaultNativeInstrumentsUserContentDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Native Instruments\User Content");
        }

        public static string GetVstWorkerLogDirectory()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Drachenkatze\PresetMagician.RemoteVstHost\Logs\");

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

        public enum LoadFxpResult
        {
            Error,
            Bank,
            Program
        }

        public static (LoadFxpResult result, string message, FXP fxp) LoadFxp(string filePath, VstPluginInfo pluginInfo)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return (LoadFxpResult.Error, "Unable to load the FXP because the path is null or empty", null);
            }

            if (!File.Exists(filePath))
            {
                return (LoadFxpResult.Error, "Unable to load the FXP because the file does not exist",
                    null);
            }


            if (!pluginInfo.Flags.HasFlag(VstPluginFlags.ProgramChunks))
            {
                return (LoadFxpResult.Error,
                    "Aborting loading because the plugin does not support ProgramChunks", null);
            }

            var fxp = new FXP();
            fxp.ReadFile(filePath);

            if (fxp.ChunkMagic != "CcnK")
            {
                return (LoadFxpResult.Error,
                    "Aborting loading because it is not an fxp or fxb file. Maybe a corrupted file? (invalid chunk)",
                    null);
            }

            var pluginUniqueId = PluginIdStringToIdNumber(fxp.FxID);
            var currentPluginId = pluginInfo.PluginID;

            if (pluginUniqueId != currentPluginId)
            {
                return (LoadFxpResult.Error,
                    "Aborting loading because it was created for a different plugin. " +
                    $"FXP/FXB plugin ID: {pluginUniqueId}, Plugin ID: {currentPluginId}",
                    null);
            }

            // Preset (Program) (.fxp) with chunk (magic = 'FPCh')
            // Bank (.fxb) with chunk (magic = 'FBCh')

            switch (fxp.FxMagic)
            {
                case "FPCh":
                    return (LoadFxpResult.Program, null, fxp);
                case "FBCh":
                    return (LoadFxpResult.Bank, null, fxp);
                default:
                    return (LoadFxpResult.Error,
                        "Cannot load the file because it is neither a bank nor a program. " +
                        $"The magic value {fxp.FxMagic} is unknown.",
                        null);
            }
        }
    }
}