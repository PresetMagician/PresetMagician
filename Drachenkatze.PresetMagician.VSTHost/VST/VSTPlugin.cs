using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class PluginInfoItem
    {
        public String Title { get; set; }

        public PluginInfoItem(string title)
        {
            Title = title;
        }
    }

    /// <summary>
    /// Contains a VSTPlugin Plugin and utility functions like MIDI calling etc.
    /// </summary>
    public class VSTPlugin : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum PresetSaveModes
        {
            // Default mode, just serialize the full bank. Active program is stored within the full bank
            Default = 0,

            // Unknown, use bank trickery
            Unknown = 1,

            // Trickery mode. Copies active program to slot 0
            BankTrickery = 2,

            // None, can't export
            None = 3
        }

        public enum PluginTypes
        {
            Effect,
            Instrument
        }

        public PluginTypes PluginType { get; set; }

        public String PluginTypeDescription
        {
            get
            {
                return PluginType.ToString();
            }
        }

        public PresetSaveModes PresetSaveMode { get; set; }

        public String PresetSaveModeDescription
        {
            get
            {
                return PresetSaveMode.ToString();
            }
        }

        public const bool PresetChunk_UseCurrentProgram = false;

        public Boolean IsOpened;

        public VstPluginContext PluginContext = null;

        public VstPluginFlags PluginFlags;

        public VSTPlugin(String dllPath)
        {
            PluginDLLPath = dllPath;
            PresetSaveMode = PresetSaveModes.None;
        }

        public bool ChunkSupport { get; set; }
        public bool IsSupported { get; set; }

        public bool IsLoaded
        {
            get
            {
                if (PluginContext != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public String LoadError { get; set; }

        public int NumPresets
        {
            get; set;
        }

        public String PluginDLLPath { get; set; }

        public String PluginName
        {
            get; set;
        }

        public String PluginVendor
        {
            get; set;
        }

        public void Dispose()
        {
            if (PluginContext != null)
            {
                PluginContext.Dispose();
            }
        }

        public void doCache()
        {
            this.PluginName = PluginContext.PluginCommandStub.GetEffectName();
            this.NumPresets = PluginContext.PluginInfo.ProgramCount;
            this.PluginVendor = PluginContext.PluginCommandStub.GetVendorString();

            if (PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth))
            {
                this.PluginType = PluginTypes.Instrument;
            }
            else
            {
                this.PluginType = PluginTypes.Effect;

                IsSupported = false;
                PresetSaveMode = PresetSaveModes.None;
                return;
            }

            if ((PluginContext.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                // Chunks not supported.
                ChunkSupport = false;
                IsSupported = false;
            }
            else
            {
                // Chunks supported.
                ChunkSupport = true;
                DeterminateVSTPresetSaveMode();

                if (PresetSaveMode != PresetSaveModes.None)
                {
                    IsSupported = true;
                }
            }
        }

        public void DeterminateVSTPresetSaveMode()
        {
            if (PluginContext.PluginInfo.ProgramCount > 1)
            {
                PresetSaveMode = PresetSaveModes.Unknown;
                if (areBankchunksConsistent())
                {
                    Debug.WriteLine(PluginName + ": bank chunks are consistent");
                    if (isCurrentProgramStoredInBankChunk())
                    {
                        Debug.WriteLine(PluginName + ": current program is stored in the bank chunk");
                        PresetSaveMode = PresetSaveModes.Default;
                        // Perfect, just put out the full bank chunk. Nothing to do here.
                    }
                    else
                    {
                        // Trick Maschine by getting the program chunk, save it to program 0, get the bank chunk,
                        // save the preset and restore the original program 0
                        if (arePresetChunksConsistent())
                        {
                            Debug.WriteLine(PluginName + ": program chunks are consistent");
                            PresetSaveMode = PresetSaveModes.BankTrickery;
                        }
                    }
                }
            }
            else
            {
                PresetSaveMode = PresetSaveModes.None;
            }
        }

        /**
         * Checks if the full bank presets are consistent. The full bank chunk should not change for the same program.
         *
         * We check it 10 times to see if the hashes are consistent. If they're not, the plugin most likely stores some
         * runtime data (like LFO state).
         *
         */

        public Boolean areBankchunksConsistent()
        {
            string firstPresetHash = getPreset(0, false, true).getPresetHash();

            for (int i = 0; i < 10; i++)
            {
                if (firstPresetHash != getPreset(0, false, true).getPresetHash())
                {
                    return false;
                }
            }

            return true;
        }

        /**
        * Checks if the full bank presets are consistent. The full bank chunk should not change for the same program.
        *
        * We check it 10 times to see if the hashes are consistent. If they're not, the plugin most likely stores some
        * runtime data (like LFO state).
        *
        */

        public Boolean arePresetChunksConsistent()
        {
            string firstPresetHash = getPreset(0, true, true).getPresetHash();

            for (int i = 0; i < 10; i++)
            {
                if (firstPresetHash != getPreset(0, true, true).getPresetHash())
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

        public Boolean isCurrentProgramStoredInBankChunk()
        {
            if (getPreset(0, false, true).getPresetHash() != getPreset(1, false, true).getPresetHash())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<PluginInfoItem> getPluginInfo()
        {
            List<PluginInfoItem> pluginInfo = new List<PluginInfoItem>();
            if (PluginContext != null)
            {
                // plugin product
                pluginInfo.Add(new PluginInfoItem("Plugin Name " + PluginContext.PluginCommandStub.GetEffectName()));
                pluginInfo.Add(new PluginInfoItem("Product " + PluginContext.PluginCommandStub.GetProductString()));
                pluginInfo.Add(new PluginInfoItem("Vendor " + PluginContext.PluginCommandStub.GetVendorString()));
                pluginInfo.Add(new PluginInfoItem("Vendor Version " + PluginContext.PluginCommandStub.GetVendorVersion().ToString()));
                pluginInfo.Add(new PluginInfoItem("Vst Support " + PluginContext.PluginCommandStub.GetVstVersion().ToString()));
                pluginInfo.Add(new PluginInfoItem("Plugin Category " + PluginContext.PluginCommandStub.GetCategory().ToString()));

                // plugin info
                pluginInfo.Add(new PluginInfoItem("Flags " + PluginContext.PluginInfo.Flags.ToString()));
                pluginInfo.Add(new PluginInfoItem("Plugin ID " + PluginContext.PluginInfo.PluginID.ToString()));
                pluginInfo.Add(new PluginInfoItem("Plugin Version " + PluginContext.PluginInfo.PluginVersion.ToString()));
                pluginInfo.Add(new PluginInfoItem("Audio Input Count " + PluginContext.PluginInfo.AudioInputCount.ToString()));
                pluginInfo.Add(new PluginInfoItem("Audio Output Count " + PluginContext.PluginInfo.AudioOutputCount.ToString()));
                pluginInfo.Add(new PluginInfoItem("Initial Delay " + PluginContext.PluginInfo.InitialDelay.ToString()));
                pluginInfo.Add(new PluginInfoItem("Program Count " + PluginContext.PluginInfo.ProgramCount.ToString()));
                pluginInfo.Add(new PluginInfoItem("Parameter Count " + PluginContext.PluginInfo.ParameterCount.ToString()));
                pluginInfo.Add(new PluginInfoItem("Tail Size " + PluginContext.PluginCommandStub.GetTailSize().ToString()));

                // can do
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Bypass + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.MidiProgramNames + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Offline + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstEvents + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstMidiEvent + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ReceiveVstTimeInfo + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstEvents + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstMidiEvent + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent)).ToString()));

                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.ConformsToWindowRules + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Metapass + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.MixDryWet + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.Multipass + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.NoRealTime + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.PlugAsChannelInsert + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.PlugAsSend + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.SendVstTimeInfo + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x1in1out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x1in2out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in1out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in2out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x2in4out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in2out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in4out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x4in8out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x8in4out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out)).ToString()));
                pluginInfo.Add(new PluginInfoItem("CanDo: " + VstPluginCanDo.x8in8out + PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out)).ToString()));

                pluginInfo.Add(new PluginInfoItem("Program: " + PluginContext.PluginCommandStub.GetProgram()));
                pluginInfo.Add(new PluginInfoItem("Program Name: " + PluginContext.PluginCommandStub.GetProgramName()));

                for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
                {
                    string name = PluginContext.PluginCommandStub.GetParameterName(i);
                    string label = PluginContext.PluginCommandStub.GetParameterLabel(i);
                    string display = PluginContext.PluginCommandStub.GetParameterDisplay(i);
                    bool canBeAutomated = PluginContext.PluginCommandStub.CanParameterBeAutomated(i);

                    pluginInfo.Add(new PluginInfoItem(String.Format("Parameter Index: {0} Parameter Name: {1} Display: {2} Label: {3} Can be automated: {4}", i, name, display, label, canBeAutomated)));
                }
            }

            return pluginInfo;
        }

        public VSTPreset getPreset(int index, Boolean isPreset)
        {
            return getPreset(index, isPreset, false);
        }

        public VSTPreset getPreset(int index, Boolean isPreset, Boolean skipSaveMode)
        {
            VSTPreset vstPreset;

            vstPreset = new VSTPreset();

            if (!skipSaveMode)
            {
                switch (PresetSaveMode)
                {
                    case PresetSaveModes.Unknown:
                    case PresetSaveModes.BankTrickery:
                        this.PluginContext.PluginCommandStub.SetProgram(0);
                        byte[] programBackup = this.PluginContext.PluginCommandStub.GetChunk(true);

                        this.PluginContext.PluginCommandStub.SetProgram(index);
                        vstPreset.PresetName = this.PluginContext.PluginCommandStub.GetProgramName();
                        byte[] realProgram = this.PluginContext.PluginCommandStub.GetChunk(true);
                        this.PluginContext.PluginCommandStub.SetProgram(0);
                        this.PluginContext.PluginCommandStub.SetChunk(realProgram, true);
                        vstPreset.PresetData = this.PluginContext.PluginCommandStub.GetChunk(false);
                        this.PluginContext.PluginCommandStub.SetChunk(programBackup, true);
                        break;

                    case PresetSaveModes.Default:
                    default:
                        this.PluginContext.PluginCommandStub.SetProgram(index);
                        vstPreset.PresetName = this.PluginContext.PluginCommandStub.GetProgramName();
                        vstPreset.PresetData = this.PluginContext.PluginCommandStub.GetChunk(false);
                        break;
                }
            }
            else
            {
                this.PluginContext.PluginCommandStub.SetProgram(index);
                vstPreset.PresetName = this.PluginContext.PluginCommandStub.GetProgramName();
                vstPreset.PresetData = this.PluginContext.PluginCommandStub.GetChunk(isPreset);
            }

            vstPreset.PreviewNote = new CannedBytes.Midi.Message.MidiNoteName("C5");
            vstPreset.ProgramNumber = this.PluginContext.PluginCommandStub.GetProgram();
            vstPreset.BankName = PluginName + " Factory";
            vstPreset.Export = true;
            vstPreset.PluginName = PluginName;
            vstPreset.PluginDLLPath = PluginDLLPath;

            return vstPreset;
        }

        public VSTPreset getPreset(int index)
        {
            return getPreset(index, false);
        }

        public void LoadFXP(string filePath)
        {
            if (filePath == null || filePath == "")
            {
                return;
            }
            // How does the GetChunk/SetChunk interface work? What information should be in those chunks?
            // How does the BeginLoadProgram and BeginLoadBank work?
            // There doesn't seem to be any restriction on what data is put in the chunks.
            // The beginLoadBank/Program methods are also part of the persistence call sequence.
            // GetChunk returns a buffer with program information of either the current/active program
            // or all programs.
            // SetChunk should read this information back in and initialize either the current/active program
            // or all programs.
            // Before SetChunk is called, the beginLoadBank/Program method is called
            // passing information on the version of the plugin that wrote the data.
            // This will allow you to support older data versions of your plugin's data or
            // even support reading other plugin's data.
            // Some hosts will call GetChunk before calling beginLoadBakn/Program and SetChunk.
            // This is an optimazation of the host to determine if the information to load is
            // actually different than the state your plugin program(s) (are) in.

            bool UseChunk = false;
            if ((PluginContext.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                // Chunks not supported.
                UseChunk = false;
            }
            else
            {
                // Chunks supported.
                UseChunk = true;
            }

            var fxp = new FXP();
            fxp.ReadFile(filePath);
            if (fxp.ChunkMagic != "CcnK")
            {
                // not a fxp or fxb file
                Console.Out.WriteLine("Error - Cannot Load. Loaded preset is not a fxp or fxb file");
                return;
            }

            int pluginUniqueID = PluginIDStringToIDNumber(fxp.FxID);
            int currentPluginID = PluginContext.PluginInfo.PluginID;
            if (pluginUniqueID != currentPluginID)
            {
                Console.Out.WriteLine("Error - Cannot Load. Loaded preset has another ID!");
            }
            else
            {
                // Preset (Program) (.fxp) with chunk (magic = 'FPCh')
                // Bank (.fxb) with chunk (magic = 'FBCh')
                if (fxp.FxMagic == "FPCh" || fxp.FxMagic == "FBCh")
                {
                    UseChunk = true;
                }
                else
                {
                    UseChunk = false;
                }
                if (UseChunk)
                {
                    // If your plug-in is configured to use chunks
                    // the Host will ask for a block of memory describing the current
                    // plug-in state for saving.
                    // To restore the state at a later stage, the same data is passed
                    // back to setChunk.
                    byte[] chunkData = fxp.ChunkDataByteArray;
                    bool beginSetProgramResult = PluginContext.PluginCommandStub.BeginSetProgram();
                    int iResult = PluginContext.PluginCommandStub.SetChunk(chunkData, true);
                    bool endSetProgramResult = PluginContext.PluginCommandStub.EndSetProgram();
                }
                else
                {
                    // Alternatively, when not using chunk, the Host will simply
                    // save all parameter values.
                    float[] parameters = fxp.Parameters;
                    bool beginSetProgramResult = PluginContext.PluginCommandStub.BeginSetProgram();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        PluginContext.PluginCommandStub.SetParameter(i, parameters[i]);
                    }
                    bool endSetProgramResult = PluginContext.PluginCommandStub.EndSetProgram();
                }
            }
        }

        public void MIDI_CC(byte Number, byte Value)
        {
            byte Cmd = 0xB0;
            MIDI(Cmd, Number, Value);
        }

        public void MIDI_NoteOff(byte Note, byte Velocity)
        {
            byte Cmd = 0x80;
            MIDI(Cmd, Note, Velocity);
        }

        public void MIDI_NoteOn(byte Note, byte Velocity)
        {
            byte Cmd = 0x90;
            MIDI(Cmd, Note, Velocity);
        }

        public void SaveFXP(string filePath)
        {
            bool UseChunk = false;
            if ((PluginContext.PluginInfo.Flags & VstPluginFlags.ProgramChunks) == 0)
            {
                // Chunks not supported.
                UseChunk = false;
            }
            else
            {
                // Chunks supported.
                UseChunk = true;
            }

            var fxp = new FXP();
            fxp.ChunkMagic = "CcnK";
            fxp.ByteSize = 0; // will be set correctly by FXP class

            if (UseChunk)
            {
                // Preset (Program) (.fxp) with chunk (magic = 'FPCh')
                fxp.FxMagic = "FPCh"; // FPCh = FXP (preset), FBCh = FXB (bank)
                fxp.Version = 1; // Format Version (should be 1)
                fxp.FxID = PluginIDNumberToIDString(PluginContext.PluginInfo.PluginID);
                fxp.FxVersion = PluginContext.PluginInfo.PluginVersion;
                fxp.ProgramCount = PluginContext.PluginInfo.ProgramCount;
                fxp.Name = PluginContext.PluginCommandStub.GetProgramName();

                byte[] chunkData = PluginContext.PluginCommandStub.GetChunk(true);
                fxp.ChunkSize = chunkData.Length;
                fxp.ChunkDataByteArray = chunkData;
            }
            else
            {
                // Preset (Program) (.fxp) without chunk (magic = 'FxCk')
                fxp.FxMagic = "FxCk"; // FxCk = FXP (preset), FxBk = FXB (bank)
                fxp.Version = 1; // Format Version (should be 1)
                fxp.FxID = PluginIDNumberToIDString(PluginContext.PluginInfo.PluginID);
                fxp.FxVersion = PluginContext.PluginInfo.PluginVersion;
                fxp.ParameterCount = PluginContext.PluginInfo.ParameterCount;
                fxp.Name = PluginContext.PluginCommandStub.GetProgramName();

                // variable no. of parameters
                var parameters = new float[fxp.ParameterCount];
                for (int i = 0; i < fxp.ParameterCount; i++)
                {
                    parameters[i] = PluginContext.PluginCommandStub.GetParameter(i);
                }
                fxp.Parameters = parameters;
            }
            fxp.WriteFile(filePath);
        }

        public void SetProgram(int programNumber)
        {
            if (programNumber < PluginContext.PluginInfo.ProgramCount && programNumber >= 0)
            {
                PluginContext.PluginCommandStub.SetProgram(programNumber);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string PluginIDNumberToIDString(int pluginUniqueID)
        {
            byte[] fxIdArray = BitConverter.GetBytes(pluginUniqueID);
            Array.Reverse(fxIdArray);
            string fxIdString = BinaryFile.ByteArrayToString(fxIdArray);
            return fxIdString;
        }

        private static int PluginIDStringToIDNumber(string fxIdString)
        {
            byte[] pluginUniqueIDArray = BinaryFile.StringToByteArray(fxIdString); // 58h8 = 946354229
            Array.Reverse(pluginUniqueIDArray);
            int pluginUniqueID = BitConverter.ToInt32(pluginUniqueIDArray, 0);
            return pluginUniqueID;
        }

        private void MIDI(byte Cmd, byte Val1, byte Val2)
        {
            /*
			 * Just a small note on the code for setting up a midi event:
			 * You can use the VstEventCollection class (Framework) to setup one or more events
			 * and then call the ToArray() method on the collection when passing it to
			 * ProcessEvents. This will save you the hassle of dealing with arrays explicitly.
			 * http://computermusicresource.com/MIDI.Commands.html
			 *
			 * Freq to Midi notes etc:
			 * http://www.sengpielaudio.com/calculator-notenames.htm
			 *
			 * Example to use NAudio Midi support
			 * http://stackoverflow.com/questions/6474388/naudio-and-midi-file-reading
			 */

            var midiData = new byte[4];
            midiData[0] = Cmd;
            midiData[1] = Val1;
            midiData[2] = Val2;
            midiData[3] = 0;    // Reserved, unused

            var vse = new VstMidiEvent(/*DeltaFrames*/ 0,
                                                /*NoteLength*/ 0,
                                                /*NoteOffset*/  0,
                                                midiData,
                                                /*Detune*/        0,
                                                /*NoteOffVelocity*/ 127);

            var ve = new VstEvent[1];
            ve[0] = vse;

            PluginContext.PluginCommandStub.ProcessEvents(ve);
        }
    }
}