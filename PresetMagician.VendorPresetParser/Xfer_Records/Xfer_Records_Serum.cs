using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.Xfer_Records
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Xfer_Records_Serum : RecursiveFXPDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1483109208};

        protected override string Extension { get; } = "fxp";

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            var dirs = new List<(string, PresetBank)>
            {
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"Xfer\Serum Presets\Presets"), GetRootBank())
            };

            var configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Xfer\Serum\Serum.cfg");
            var demoConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Xfer\Serum\SerumDemo.cfg");

            if (File.Exists(configFile))
            {
                var lines = File.ReadAllLines(configFile);

                foreach (var cfgLine in lines)
                {
                    var line = cfgLine.Replace(Convert.ToChar(0x0).ToString(), "").Trim();
                    var closingBracketIndex = line.IndexOf("]", StringComparison.Ordinal);
                    
                    var openingBracketIndex = line.IndexOf("[", StringComparison.Ordinal);
                    var rest = "";
                    if (closingBracketIndex + 1 < line.Length)
                    {
                        rest = line.Substring(closingBracketIndex + 1).Trim();
                    }

                    if (closingBracketIndex == -1 || openingBracketIndex == -1 || rest.Length != 0)
                    {
                        continue;
                    }

                    var data = line
                        .Substring(openingBracketIndex + 1, closingBracketIndex - openingBracketIndex - 1)
                        .Replace("/", @"\").Trim();

                    if (string.IsNullOrWhiteSpace(data) || data == "Default" )
                    {
                        continue;
                    }
                    if (Directory.Exists(data))
                    {
                        dirs.Add((data, GetRootBank()));
                        Logger.Info($"Using additional directory {data} for presets");
                    }
                    else
                    {
                        Logger.Warning($"Found directory {data} in the serum config, but it doesn't exist.");
                    }

                }
            }
            else
            {
                if (!File.Exists(demoConfigFile))
                {
                    Logger.Warning($"Could not find {configFile} or {demoConfigFile}");
                }
            }
            

            return dirs;
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}