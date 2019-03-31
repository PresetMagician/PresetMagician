using System.Collections.Generic;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser
{
    public partial class AbstractVendorPresetParser
    {
        public enum PresetSaveModes
        {
            // Default mode, just serialize the full bank. Active program is stored within the full bank
            FullBank = 0,

            // Fallback, use bank trickery
            Fallback = 1,

            // Trickery mode. Copies active program to slot 0
            BankTrickery = 2,

            // None, can't export
            None = 3,

            // Not yet checked
            NotYetDetermined = 4
        }

        protected readonly Dictionary<string, string> PresetHashes = new Dictionary<string, string>();

        protected PresetSaveModes PresetSaveMode = PresetSaveModes.NotYetDetermined;


        protected int GetAdditionalBanksPresetCount()
        {
            var presetCount = 0;
            foreach (var bank in AdditionalBankFiles)
            {
                var result = VstUtils.LoadFxp(bank.Path, PluginInstance.Plugin.PluginInfo.ToNonSurrogate());

                if (result.result == VstUtils.LoadFxpResult.Error)
                {
                    continue;
                }

                if (result.result == VstUtils.LoadFxpResult.Program)
                {
                    presetCount++;
                }
                else if (result.result == VstUtils.LoadFxpResult.Bank)
                {
                    var ranges = bank.GetProgramRanges();

                    if (ranges.Count == 0)
                    {
                        presetCount += PluginInstance.Plugin.PluginInfo.ProgramCount;
                    }
                    else
                    {
                        foreach (var (_, length) in ranges)
                        {
                            presetCount += length;
                        }
                    }
                }
            }

            return presetCount;
        }

        private async Task ParseAdditionalBanks()
        {
            await PluginInstance.LoadPlugin();
            DeterminateVstPresetSaveMode();

            foreach (var bank in AdditionalBankFiles)
            {
                var result = LoadFxp(bank.Path);

                if (result == VstUtils.LoadFxpResult.Error)
                {
                    continue;
                }

                var presetBank = RootBank.CreateRecursive(bank.BankName);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (result)
                {
                    case VstUtils.LoadFxpResult.Program:
                        await GetPresets(presetBank, 0, 1, bank.Path);
                        break;
                    case VstUtils.LoadFxpResult.Bank:
                    {
                        var ranges = bank.GetProgramRanges();

                        if (ranges.Count == 0)
                        {
                            await GetPresets(presetBank, 0, PluginInstance.Plugin.PluginInfo.ProgramCount, bank.Path);
                        }
                        else
                        {
                            foreach (var (start, length) in ranges)
                            {
                                await GetPresets(presetBank, start - 1, length, bank.Path);
                            }
                        }

                        break;
                    }
                }
            }
        }

        protected async Task GetPresets(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            switch (PresetSaveMode)
            {
                case PresetSaveModes.Fallback:
                case PresetSaveModes.BankTrickery:
                    await GetPresetsUsingBankTrickery(bank, start, numPresets, sourceFile);
                    break;
                case PresetSaveModes.FullBank:
                case PresetSaveModes.None:
                    await GetPresetsUsingFullBank(bank, start, numPresets, sourceFile);
                    break;
            }
        }


        public VstUtils.LoadFxpResult LoadFxp(string filePath)
        {
            var result = VstUtils.LoadFxp(filePath, PluginInstance.Plugin.PluginInfo.ToNonSurrogate());

            if (result.result == VstUtils.LoadFxpResult.Error)
            {
                Logger.Error($"Error loading FXB/FXP {filePath}: {result.message}");
                return result.result;
            }

            // If your plug-in is configured to use chunks
            // the Host will ask for a block of memory describing the current
            // plug-in state for saving.
            // To restore the state at a later stage, the same data is passed
            // back to setChunk.
            var chunkData = result.fxp.ChunkDataByteArray;

            PluginInstance.SetProgram(0);
            PluginInstance.SetChunk(chunkData, result.result == VstUtils.LoadFxpResult.Program);

            return result.result;
        }

        protected async Task GetPresetsUsingFullBank(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                Logger.Error("GetPresets start index is less than 0, ignoring. This is probably a bug or a " +
                    "misconfiguration. Please report this including the full log file.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > PluginInstance.Plugin.PluginInfo.ProgramCount)
            {
                Logger.Error(
                    $"Tried to retrieve presets between the index {start} and {endIndex}, but this would exceed maximum "+
                    $"program count of {PluginInstance.Plugin.PluginInfo.ProgramCount}, ignoring. You might wish to "+
                    "report this as a bug.");
                return;
            }

            for (var index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(index);

                var preset = new PresetParserMetadata
                {
                    PresetName = PluginInstance.GetCurrentProgramName(),
                    BankPath = bank.BankPath,
                    SourceFile = sourceFile + ":" + index,
                    Plugin = PluginInstance.Plugin
                };

                await DataPersistence.PersistPreset(preset, PluginInstance.GetChunk(false));
            }
        }

        protected async Task GetPresetsUsingBankTrickery(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                Logger.Error("GetPresets start index is less than 0, ignoring. This is probably a bug or a " +
                             "misconfiguration. Please report this including the full log file.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > PluginInstance.Plugin.PluginInfo.ProgramCount)
            {
                Logger.Error(
                    $"Tried to retrieve presets between the index {start} and {endIndex}, but this would exceed maximum "+
                    $"program count of {PluginInstance.Plugin.PluginInfo.ProgramCount}, ignoring. You might wish to "+
                    "report this as a bug.");
                return;
            }

            for (var index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(0);
                var programBackup = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(index);
                var programName = PluginInstance.GetCurrentProgramName();
                var fullSourceFile = sourceFile + ":" + index;
                var vstPreset = new PresetParserMetadata
                {
                    SourceFile = fullSourceFile,
                    BankPath = bank.BankPath,
                    PresetName = programName,
                    Plugin = PluginInstance.Plugin
                };


                var realProgram = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(0);

                PluginInstance.SetChunk(realProgram, true);
                var presetData = PluginInstance.GetChunk(false);
                PluginInstance.SetChunk(programBackup, true);

                var hash = HashUtils.getIxxHash(realProgram);

                if (PresetHashes.ContainsKey(hash))
                {
                    Logger.Warning(
                        $"Skipping program {index} with name {programName} because a program with the same data "+
                        $"was already added ({PresetHashes[hash]}. Please report this if you think if it's a bug.");
                }
                else
                {
                    PresetHashes.Add(hash, fullSourceFile + " "+programName);
                    await DataPersistence.PersistPreset(vstPreset, presetData);
                }
            }
        }

        public void DeterminateVstPresetSaveMode()
        {
            if (PresetSaveMode != PresetSaveModes.NotYetDetermined)
            {
                return;
            }

            if (PluginInstance.Plugin.PluginType == Plugin.PluginTypes.Unknown)
            {
                PresetSaveMode = PresetSaveModes.None;
                Logger.Info(
                    "Unknown plugin type, setting preset save mode to none");
                return;
            }

            if ((PluginInstance.Plugin.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                PresetSaveMode = PresetSaveModes.None;
                Logger.Info(
                    "Plugin does not support program chunks, setting preset save mode to none");
                return;
            }

            if (PluginInstance.Plugin.PluginInfo.ProgramCount > 1)
            {
                PluginInstance.LoadPlugin().Wait();
                Logger.Debug(PluginInstance.Plugin.PluginName +
                                                   ": Program count is greater than 1, checking for preset save mode");

                if (AreChunksNull(false))
                {
                    // Bank chunks are null, can't use for NKS
                    PresetSaveMode = PresetSaveModes.None;
                    return;
                }

                if (!AreChunksConsistent(false))
                {
                    PresetSaveMode = PresetSaveModes.Fallback;
                    Logger.Info(
                        "Using preset save mode fallback");
                    return;
                }

                Logger.Debug(PluginInstance.Plugin.PluginName + ": bank chunks are consistent");

                if (IsCurrentProgramStoredInBankChunk())
                {
                    Logger.Debug(PluginInstance.Plugin.PluginName +
                                                       ": current program is stored in the bank chunk");
                    PresetSaveMode = PresetSaveModes.FullBank;
                    Logger.Info(
                        "Using preset save mode full bank");
                    return;

                    // Perfect, just put out the full bank chunk. Nothing to do here.
                }

                // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                // save the preset and restore the original program 0
                if (!AreChunksNull(true))
                {
                    if (AreChunksConsistent(true))
                    {
                        Logger.Debug(PluginInstance.Plugin.PluginName +
                                                           ": program chunks are consistent");
                        PresetSaveMode = PresetSaveModes.BankTrickery;
                        Logger.Info(
                            "Using preset save mode bank trickery");
                        return;
                    }

                    PresetSaveMode = PresetSaveModes.Fallback;
                    Logger.Info(
                        "Using preset save mode fallback");
                    return;
                }

                PresetSaveMode = PresetSaveModes.None;
                Logger.Info(
                    "Using preset save mode none");
                return;
            }

            Logger.Info(
                "Plugin reported a program count of 0 or 1, using preset save mode none");
            PresetSaveMode = PresetSaveModes.None;
        }

        /**
         * Checks if the chunks are null
         */
        public bool AreChunksNull(bool isPreset)
        {
            Logger.Debug(isPreset
                ? $"{PluginInstance.Plugin.PluginName}: checking if bank chunks are null"
                : $"{PluginInstance.Plugin.PluginName}: checking if program chunks are null");

            PluginInstance.SetProgram(0);

            var chunk = PluginInstance.GetChunk(isPreset);

            return chunk == null;
        }

        /**
         * Checks if the full bank presets are consistent. The full bank chunk should not change for the same program.
         *
         * We check it 10 times to see if the hashes are consistent. If they're not, the plugin most likely stores some
         * runtime data (like LFO state).
         *
         */
        public bool AreChunksConsistent(bool isPreset)
        {
            Logger.Debug(isPreset
                ? $"{PluginInstance.Plugin.PluginName}: checking if bank chunks are consistent"
                : $"{PluginInstance.Plugin.PluginName}: checking if program chunks are consistent");

            PluginInstance.SetProgram(0);

            var chunk = PluginInstance.GetChunk(isPreset);

            if (chunk == null)
            {
                return false;
            }

            var firstPresetHash =
                HashUtils.getIxxHash(chunk);

            Logger.Debug(PluginInstance.Plugin.PluginName + ": hash for program 0 is " +
                                               firstPresetHash);

            for (var i = 0; i < 10; i++)
            {
                PluginInstance.SetProgram(0);
                chunk = PluginInstance.GetChunk(isPreset);

                if (chunk == null)
                {
                    return false;
                }

                if (firstPresetHash !=
                    HashUtils.getIxxHash(chunk))
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Checks if the current program number is stored in the bank chunk. Some VSTs do it (like V-Station),
         * others don't (which should be the norm).
         *
         * Interpret the return value "true" as "uncertain"
         */

        public bool IsCurrentProgramStoredInBankChunk()
        {
            PluginInstance.SetProgram(0);
            var firstHash = HashUtils.getIxxHash(PluginInstance.GetChunk(false));

            PluginInstance.SetProgram(1);
            var secondHash =
                HashUtils.getIxxHash(PluginInstance.GetChunk(false));

            if (firstHash != secondHash)
            {
                return true;
            }

            return false;
        }
    }
}