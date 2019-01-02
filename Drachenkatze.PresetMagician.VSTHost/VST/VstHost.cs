using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using Catel.Collections;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class NoOfflineSupportException : Exception
    {
    }

    public class EffectsNotSupportedException : Exception
    {
    }

    public class NoRealtimeProcessingException : Exception
    {
    }

    public class VstHost
    {
        //public VSTPluginExport pluginExporter;

        public VstHost()
        {
            //pluginExporter = new VSTPluginExport();
        }

        /// <summary>
        ///     Returns all found DLLs for a specific directory
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <returns></returns>
        public ObservableCollection<string> EnumeratePlugins(string pluginDirectory)
        {
            var vstPlugins = new ObservableCollection<string>();
            foreach (var file in Directory.EnumerateFiles(
                pluginDirectory, "*.dll", SearchOption.AllDirectories))
            {
                vstPlugins.Add(file);
            }

            return vstPlugins;
        }

        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        public const int BlockSize = 512;
        public const float SampleRate = 44100f;


        public void LoadVST(IVstPlugin vst)
        {
            var hostCommandStub = new HostCommandStub();

            try
            {
                var ctx = VstPluginContext.Create(vst.DllPath, hostCommandStub);

                vst.PluginContext = ctx;
                ctx.Set("PluginPath", vst.DllPath);
                ctx.Set("HostCmdStub", hostCommandStub);
                ctx.PluginCommandStub.SetBlockSize(BlockSize);
                ctx.PluginCommandStub.SetSampleRate(SampleRate);
                
                ctx.PluginCommandStub.Open();
                ctx.PluginCommandStub.StartProcess();
                vst.PluginContext.PluginCommandStub.MainsChanged(true);
                IdleLoop(vst,1024);
                vst.OnLoaded();
            }
            catch (Exception e)
            {
                vst.OnLoadError(e);
            }
        }

        public void IdleLoop(IVstPlugin plugin, int loops)
        {
            var ctx = plugin.PluginContext;
            var outputCount = ctx.PluginInfo.AudioOutputCount;
            var inputCount = ctx.PluginInfo.AudioInputCount;

            using (var inputMgr = new VstAudioBufferManager(inputCount, BlockSize))
            {
                using (var outputMgr = new VstAudioBufferManager(outputCount, BlockSize))
                {
                    var outputBuffers = outputMgr.ToArray();
                    var inputBuffers = inputMgr.ToArray();
                    int k;

                    for (k = 0; k < loops; k++)
                    {
                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                    }
                }
            }
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            var hostCmdStub = (HostCommandStub) sender;

            // can be null when called from inside the plugin main entry point.
            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called:" + e.Message);
            }
        }

        private void MIDI(IVstPlugin plugin, byte Cmd, byte Val1, byte Val2)
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

            plugin.PluginContext.PluginCommandStub.ProcessEvents(ve);
        }

        public void MIDI_CC(IVstPlugin plugin, byte Number, byte Value)
        {
            byte Cmd = 0xB0;
            MIDI(plugin, Cmd, Number, Value);
        }

        public void MIDI_NoteOff(IVstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x80;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void MIDI_NoteOn(IVstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x90;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void UnloadVST(IVstPlugin vst)
        {
            vst.PluginContext?.Dispose();
            vst.PluginContext = null;
        }
    }
}