using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.InitialAudio
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class InitialAudio_Sektor : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1399551090};

        protected override string Extension { get; } = "sektorPreset";

        public override string Remarks { get; set; } =
            "This is a beta of the Sektor Synth preset parser and might not work correctly.";

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetExpansionsDirectory()}";
            base.Init();
        }

        private string GetExpansionsDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"IgniteVST\Sektor\Expansions");
        }

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            var dirs = new List<(string directory, PresetBank presetBank)>();

            foreach (var dir in Directory.EnumerateDirectories(GetExpansionsDirectory()))
            {
                var presetDirectory = Path.Combine(dir, "Presets");

                if (Directory.Exists(presetDirectory))
                {
                    var bank = RootBank.CreateRecursive(Path.GetFileName(dir));
                    dirs.Add((presetDirectory, bank));
                }
            }

            return dirs;
        }

        protected override string GetParseDirectory()
        {
            // Should not be called
            throw new NotImplementedException();
        }
    }
}