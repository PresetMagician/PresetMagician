using System;
using System.Diagnostics;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Anotar.Catel;

namespace Drachenkatze.PresetMagician.VendorPresetParser.StandardVST
{
    public class AbstractStandardVstPresetParser : AbstractVendorPresetParser
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
            None = 3
        }

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
                else
                {
                    // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                    // save the preset and restore the original program 0
                    if (AreChunksConsistent(true))
                    {
                        LogTo.Debug(VstPlugin.PluginName + ": program chunks are consistent");
                        return PresetSaveModes.BankTrickery;
                    }
                }

                return PresetSaveModes.Fallback;
            }
            else
            {
                return PresetSaveModes.None;
            }
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
            
            string firstPresetHash =
                HashUtils.getFormattedSHA256Hash(VstPlugin.PluginContext.PluginCommandStub.GetChunk(isPreset));

            LogTo.Debug(VstPlugin.PluginName + ": hash for program 0 is "+firstPresetHash);
            
            for (int i = 0; i < 10; i++)
            {
                VstPlugin.PluginContext.PluginCommandStub.SetProgram(0);
                if (firstPresetHash != HashUtils.getFormattedSHA256Hash(VstPlugin.PluginContext.PluginCommandStub.GetChunk(isPreset)))
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
            else
            {
                return false;
            }
        }
    }
}