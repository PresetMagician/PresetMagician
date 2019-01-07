using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Drachenkatze.PresetMagician.Utils;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    

    /// <summary>
    /// Contains a VSTPlugin Plugin and utility functions like MIDI calling etc.
    /// </summary>
    public class VSTPlugin : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int PluginId { get; set; }

        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        public String PluginTypeDescription
        {
            get
            {
                return PluginType.ToString();
            }
        }

        public const bool PresetChunk_UseCurrentProgram = false;

        public Boolean IsOpened;

        public VstPluginContext PluginContext = null;

        public VstPluginFlags PluginFlags;

        public VSTPlugin(String dllPath)
        {
            PluginDLLPath = dllPath;
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
            this.PluginId = PluginContext.PluginInfo.PluginID;

            if (PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth))
            {
                this.PluginType = PluginTypes.Instrument;
            }
            else
            {
                this.PluginType = PluginTypes.Effect;
            }

            // Scan for preset implementations here
        }

        

        

        public void SetProgram(int programNumber)
        {
            if (programNumber < PluginContext.PluginInfo.ProgramCount && programNumber >= 0)
            {
                PluginContext.PluginCommandStub.SetProgram(programNumber);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

        
    }
}