using System.Collections.Generic;
using System.IO;
using Anotar.Catel;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public abstract class AbstractStandardVstPresetParser : AbstractVendorPresetParser
    {
        protected List<string> PresetHashes = new List<string>();
        public override bool SupportsAdditionalBankFiles { get; set; } = true;
        public override List<BankFile> AdditionalBankFiles { get; } = new List<BankFile>();
        
        public enum PresetSaveModes
        {
            // Default mode, just serialize the full bank. Active program is stored within the full bank
            FullBank = 0,

            // Fallback, use bank trickery
            Fallback = 1,

            // Trickery mode. Copies active program to slot 0
            BankTrickery = 2,

            // None, can't export
            None = 3
        }

        public IPresetBank FindOrCreateBank(string bankPath)
        {
            var result = RootBank.FindBankPath(bankPath);

            if (result != null)
            {
                return result;
            }

            return RootBank.CreateRecursive(bankPath);
        }
        
        protected void ParseAdditionalBanks()
        {
            foreach (var bank in AdditionalBankFiles)
            {
                var result = LoadFxp(bank.Path);

                if (result == LoadFxpResult.Error)
                {
                    continue;
                }

                var presetBank = FindOrCreateBank(bank.BankName);

                if (result == LoadFxpResult.Program)
                {
                    GetPresets(presetBank, 0, 1);
                } else if (result == LoadFxpResult.Bank)
                {
                    var ranges = bank.GetProgramRanges();

                    if (ranges.Count == 0)
                    {
                        GetPresets(presetBank, 0, VstPlugin.PluginContext.PluginInfo.ProgramCount);
                    }
                    else
                    {
                        foreach (var (start, length) in ranges)
                        {
                            GetPresets(presetBank, start-1, length); 
                        }
                    }
                }
            }
        }

        protected abstract void GetPresets(IPresetBank bank, int start, int numPresets);

        public PresetSaveModes DeterminateVSTPresetSaveMode()
        {
            if (VstPlugin.PluginType == VstHost.PluginTypes.Unknown)
            {
                return PresetSaveModes.None;
            }

            if ((VstPlugin.PluginContext.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                return PresetSaveModes.None;
            }

            if (VstPlugin.PluginContext.PluginInfo.ProgramCount > 1)
            {
                LogTo.Debug(VstPlugin.PluginName + ": Program count is greater than 1, checking for preset save mode");
                if (!AreChunksConsistent(false))
                {
                    return PresetSaveModes.Fallback;
                }

                LogTo.Debug(VstPlugin.PluginName + ": bank chunks are consistent");

                if (IsCurrentProgramStoredInBankChunk())
                {
                    LogTo.Debug(VstPlugin.PluginName + ": current program is stored in the bank chunk");
                    return PresetSaveModes.FullBank;

                    // Perfect, just put out the full bank chunk. Nothing to do here.
                }

                // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                // save the preset and restore the original program 0
                if (AreChunksConsistent(true))
                {
                    LogTo.Debug(VstPlugin.PluginName + ": program chunks are consistent");
                    return PresetSaveModes.BankTrickery;
                }

                return PresetSaveModes.Fallback;
            }

            return PresetSaveModes.None;
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
            LogTo.Debug(VstPlugin.PluginName + ": checking if chunks are consistent");
            VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);

            var chunk = VstPlugin.PluginContext.PluginCommandStub.GetChunk(isPreset);

            if (chunk == null)
            {
                return false;
            }
            
            string firstPresetHash =
                HashUtils.getFormattedSHA256Hash(chunk);

            LogTo.Debug(VstPlugin.PluginName + ": hash for program 0 is " + firstPresetHash);

            for (int i = 0; i < 10; i++)
            {
                VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
                chunk = VstPlugin.PluginContext.PluginCommandStub.GetChunk(isPreset);

                if (chunk == null)
                {
                    return false;
                }
                
                if (firstPresetHash !=
                    HashUtils.getFormattedSHA256Hash(chunk))
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
            VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
            var firstHash = HashUtils.getFormattedSHA256Hash(VstPlugin.PluginContext.PluginCommandStub.GetChunk(false));

            VstPlugin.PluginContext.PluginCommandStub.SetProgram(1);
            var secondHash =
                HashUtils.getFormattedSHA256Hash(VstPlugin.PluginContext.PluginCommandStub.GetChunk(false));

            if (firstHash != secondHash)
            {
                return true;
            }

            return false;
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
                return LoadFxpResult.Error;
            }

            if (!File.Exists(filePath))
            {
                LogTo.Error($"Unable to load {filePath} because it doesn't exist");
                return LoadFxpResult.Error;
            }


            if ((VstPlugin.PluginContext.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                LogTo.Error($"Aborting because the plugin does not support ProgramChunks");
                return LoadFxpResult.Error;
            }


            var fxp = new FXP();
            fxp.ReadFile(filePath);

            if (fxp.ChunkMagic != "CcnK")
            {
                // not a fxp or fxb file
                LogTo.Error($"Cannot load {filePath} because it is not an fxp or fxb file");
                return LoadFxpResult.Error;
            }

            int pluginUniqueID = VstUtils.PluginIDStringToIDNumber(fxp.FxID);
            int currentPluginID = VstPlugin.PluginContext.PluginInfo.PluginID;
            if (pluginUniqueID != currentPluginID)
            {
                LogTo.Error(
                    $"Cannot load {filePath} because it is not meant for this plugin! FXP/FXB plugin ID: {pluginUniqueID}, Plugin ID: {currentPluginID}");
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
                    LogTo.Error($"Cannot load {filePath} because the magic value {fxp.FxMagic} is unknown");
                    return LoadFxpResult.Error;
            }
            


            // If your plug-in is configured to use chunks
            // the Host will ask for a block of memory describing the current
            // plug-in state for saving.
            // To restore the state at a later stage, the same data is passed
            // back to setChunk.
            var chunkData = fxp.ChunkDataByteArray;
            VstPlugin.PluginContext.PluginCommandStub.BeginSetProgram();
            
            VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
            VstPlugin.PluginContext.PluginCommandStub.SetChunk(chunkData, usePreset);

            return successResult;
        }
    }

}