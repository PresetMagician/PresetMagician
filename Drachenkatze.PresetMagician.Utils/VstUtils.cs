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
    }
}