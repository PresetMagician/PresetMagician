using System.Collections.Generic;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using PresetMagician.Core.Models;

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
                PluginInstance.Plugin.Logger.Error($"{filePath} {result.message}");
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
            if (PresetSaveMode != PresetSaveModes.NotYetDetermined)
            {
                return;
            }
            
            if (PluginInstance.Plugin.PluginType == Plugin.PluginTypes.Unknown)
            {
                PresetSaveMode = PresetSaveModes.None;
                PluginInstance.Plugin.Logger.Info(
                    $"Unknown plugin type, setting preset save mode to none");
                return;
            }

            if ((PluginInstance.Plugin.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                PresetSaveMode = PresetSaveModes.None;
                PluginInstance.Plugin.Logger.Info(
                    $"Plugin does not support program chunks, setting preset save mode to none");
                return;
            }

            if (PluginInstance.Plugin.PluginInfo.ProgramCount > 1)
            {
                PluginInstance.LoadPlugin().Wait();
                PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
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
                    PluginInstance.Plugin.Logger.Info(
                        $"Using preset save mode fallback");
                    return;
                }

                PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName + ": bank chunks are consistent");

                if (IsCurrentProgramStoredInBankChunk())
                {
                    PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                                       ": current program is stored in the bank chunk");
                    PresetSaveMode = PresetSaveModes.FullBank;
                    PluginInstance.Plugin.Logger.Info(
                        $"Using preset save mode full bank");
                    return;

                    // Perfect, just put out the full bank chunk. Nothing to do here.
                }

                // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                // save the preset and restore the original program 0
                if (!AreChunksNull(true))
                {
                    if (AreChunksConsistent(true))
                    {
                        PluginInstance.Plugin.Logger.Debug(PluginInstance.Plugin.PluginName +
                                                           ": program chunks are consistent");
                        PresetSaveMode = PresetSaveModes.BankTrickery;
                        PluginInstance.Plugin.Logger.Info(
                            $"Using preset save mode bank trickery");
                        return;
                    }
                    
                    PresetSaveMode = PresetSaveModes.Fallback;
                    PluginInstance.Plugin.Logger.Info(
                        $"Using preset save mode fallback");
                    return;
                }
                
                PresetSaveMode = PresetSaveModes.None;
                PluginInstance.Plugin.Logger.Info(
                    $"Using preset save mode none");
                return;

                
            }

            PluginInstance.Plugin.Logger.Info(
                $"Plugin reported a program count of 0 or 1, using preset save mode none");
            PresetSaveMode = PresetSaveModes.None;
        }
        
        /**
         * Checks if the chunks are null
         */
        public bool AreChunksNull(bool isPreset)
        {
            PluginInstance.Plugin.Logger.Debug(isPreset
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
            PluginInstance.Plugin.Logger.Debug(isPreset
                ? $"{PluginInstance.Plugin.PluginName}: checking if bank chunks are consistent"
                : $"{PluginInstance.Plugin.PluginName}: checking if program chunks are consistent");

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