using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public static class VstPathScanner
    {
        private static readonly string[] vstPaths =
        {
            @"\VSTPlugins",
            @"\Steinberg\VSTPlugins",
            @"\Common Files\VST2",
            @"\Common Files\Steinberg\VST2",
            @"\Native Instruments\VSTPlugins 64 bit"
        };


        public static List<string> getCommonVSTPluginDirectories()
        {
            List<string> vstLocations = new List<string>();

            foreach (String vstPath in vstPaths)
            {
                var vstLocation = Environment.GetEnvironmentVariable("ProgramFiles") + vstPath;
                if (Directory.Exists(vstLocation))
                {
                    vstLocations.Add(vstLocation);
                }
            }

            return vstLocations;
        }
    }
}