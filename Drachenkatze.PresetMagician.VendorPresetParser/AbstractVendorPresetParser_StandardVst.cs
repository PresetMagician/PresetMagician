using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public partial class AbstractVendorPresetParser
    {
        protected readonly List<string> PresetHashes = new List<string>();

        protected PresetSaveModes PresetSaveMode = PresetSaveModes.NotYetDetermined;

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


        protected virtual int GetAdditionalBanksPresetCount()
        {
            var presetCount = 0;
            foreach (var bank in AdditionalBankFiles)
            {
                var result = LoadFxp(bank.Path);

                if (result == LoadFxpResult.Error)
                {
                    continue;
                }

                if (result == LoadFxpResult.Program)
                {
                    presetCount++;
                }
                else if (result == LoadFxpResult.Bank)
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

                if (result == LoadFxpResult.Error)
                {
                    continue;
                }

                var presetBank = RootBank.CreateRecursive(bank.BankName);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (result)
                {
                    case LoadFxpResult.Program:
                        await GetPresets(presetBank, 0, 1, bank.Path);
                        break;
                    case LoadFxpResult.Bank:
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

        public enum LoadFxpResult
        {
            Error,
            Bank,
            Program
        }

        public LoadFxpResult LoadFxp(string filePath)
        {
            LoadFxpResult successResult;

            if (string.IsNullOrEmpty(filePath))
            {
                PluginInstance.Plugin.Logger.Error($"Unable to load an FXP because the path is null or empty");
                return LoadFxpResult.Error;
            }

            if (!File.Exists(filePath))
            {
                PluginInstance.Plugin.Logger.Error($"Unable to load {filePath} because it doesn't exist");
                return LoadFxpResult.Error;
            }


            if ((PluginInstance.Plugin.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                PluginInstance.Plugin.Logger.Error($"Aborting because the plugin does not support ProgramChunks");
                return LoadFxpResult.Error;
            }


            var fxp = new FXP();
            fxp.ReadFile(filePath);

            if (fxp.ChunkMagic != "CcnK")
            {
                // not a fxp or fxb file
                PluginInstance.Plugin.Logger.Error($"Cannot load {filePath} because it is not an fxp or fxb file");
                return LoadFxpResult.Error;
            }

            var pluginUniqueId = VstUtils.PluginIdStringToIdNumber(fxp.FxID);
            var currentPluginId = PluginInstance.Plugin.VstPluginId;

            if (pluginUniqueId != currentPluginId)
            {
                PluginInstance.Plugin.Logger.Error(
                    $"Cannot load {filePath} because it is not meant for this plugin! FXP/FXB plugin ID: {pluginUniqueId}, Plugin ID: {currentPluginId}");
                return LoadFxpResult.Error;
            }


            // Preset (Program) (.fxp) with chunk (magic = 'FPCh')
            // Bank (.fxb) with chunk (magic = 'FBCh')
            bool usePreset;

            switch (fxp.FxMagic)
            {
                case "FPCh":
                    usePreset = true;
                    successResult = LoadFxpResult.Program;
                    break;
                case "FBCh":
                    usePreset = false;
                    successResult = LoadFxpResult.Bank;
                    break;
                default:
                    PluginInstance.Plugin.Logger.Error(
                        $"Cannot load {filePath} because the magic value {fxp.FxMagic} is unknown");
                    return LoadFxpResult.Error;
            }


            // If your plug-in is configured to use chunks
            // the Host will ask for a block of memory describing the current
            // plug-in state for saving.
            // To restore the state at a later stage, the same data is passed
            // back to setChunk.
            var chunkData = fxp.ChunkDataByteArray;

            PluginInstance.SetProgram(0);
            PluginInstance.SetChunk(chunkData, usePreset);

            return successResult;
        }

        protected async Task GetPresetsUsingFullBank(PresetBank bank, int start, int numPresets, string sourceFile)
        {
            if (start < 0)
            {
                PluginInstance.Plugin.Logger.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > PluginInstance.Plugin.PluginInfo.ProgramCount)
            {
                PluginInstance.Plugin.Logger.Error(
                    $"GetPresets between {start} and {endIndex} would exceed maximum program count of {PluginInstance.Plugin.PluginInfo.ProgramCount}, ignoring.");
                return;
            }

            for (var index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(index);

                var preset = new Preset
                {
                    PresetName = PluginInstance.GetCurrentProgramName(),
                    PresetBank = bank,
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
                PluginInstance.Plugin.Logger.Error("GetPresets start index is less than 0, ignoring.");
                return;
            }

            var endIndex = start + numPresets;

            if (endIndex > PluginInstance.Plugin.PluginInfo.ProgramCount)
            {
                PluginInstance.Plugin.Logger.Error(
                    $"GetPresets between {start} and {endIndex} would exceed maximum program count of {PluginInstance.Plugin.PluginInfo.ProgramCount}, ignoring.");
                return;
            }

            for (int index = start; index < endIndex; index++)
            {
                PluginInstance.SetProgram(0);
                var programBackup = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(index);

                var vstPreset = new Preset
                {
                    SourceFile = sourceFile + ":" + index,
                    PresetBank = bank,
                    PresetName = PluginInstance.GetCurrentProgramName(),
                    Plugin = PluginInstance.Plugin
                };


                byte[] realProgram = PluginInstance.GetChunk(true);
                PluginInstance.SetProgram(0);

                PluginInstance.SetChunk(realProgram, true);
                var presetData = PluginInstance.GetChunk(false);
                PluginInstance.SetChunk(programBackup, true);

                var hash = HashUtils.getIxxHash(realProgram);

                if (PresetHashes.Contains(hash))
                {
                    PluginInstance.Plugin.Logger.Debug(
                        $"Skipping program {index} because the preset already seem to exist");
                }
                else
                {
                    PresetHashes.Add(hash);
                    await DataPersistence.PersistPreset(vstPreset, presetData);
                }
            }
        }

        public void DeterminateVstPresetSaveMode()
        {
            if (PluginInstance.Plugin.PluginType == Plugin.PluginTypes.Unknown)
            {
                PresetSaveMode = PresetSaveModes.None;
            }

            if ((PluginInstance.Plugin.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                PresetSaveMode = PresetSaveModes.None;
            }

            if (PluginInstance.Plugin.PluginInfo.ProgramCount > 1)
            {
                PluginInstance.LoadPlugin().Wait();
                PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                                   ": Program count is greater than 1, checking for preset save mode");
                if (!AreChunksConsistent(false))
                {
                    PresetSaveMode = PresetSaveModes.Fallback;
                }

                PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName + ": bank chunks are consistent");

                if (IsCurrentProgramStoredInBankChunk())
                {
                    PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                                       ": current program is stored in the bank chunk");
                    PresetSaveMode = PresetSaveModes.FullBank;

                    // Perfect, just put out the full bank chunk. Nothing to do here.
                }

                // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                // save the preset and restore the original program 0
                if (AreChunksConsistent(true))
                {
                    PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                                       ": program chunks are consistent");
                    PresetSaveMode = PresetSaveModes.BankTrickery;
                }

                PresetSaveMode = PresetSaveModes.Fallback;
            }

            PresetSaveMode = PresetSaveModes.None;
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
            PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                               ": checking if chunks are consistent");
            PluginInstance.SetProgram(0);

            var chunk = PluginInstance.GetChunk(isPreset);

            if (chunk == null)
            {
                return false;
            }

            string firstPresetHash =
                HashUtils.getIxxHash(chunk);

            PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName + ": hash for program 0 is " +
                                               firstPresetHash);

            for (int i = 0; i < 10; i++)
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