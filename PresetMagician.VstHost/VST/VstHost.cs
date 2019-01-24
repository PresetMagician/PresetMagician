using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Drachenkatze.PresetMagician.VSTHost;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;
using MethodTimer;
using PresetMagician.VstHost.Util;
using SharedModels;

namespace PresetMagician.VstHost.VST
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

    public class VstHost : IVstHost
    {
        private Timer _audioTimer;
        private Timer _guiTimer;

        public VstHost(bool useTimer = false)
        {
            if (useTimer)
            {
                _audioTimer = new Timer();
                _audioTimer.Elapsed += AudioTimerOnElapsed;
                _audioTimer.Interval = 50;
                _audioTimer.Enabled = true;
                _audioTimer.AutoReset = false;

                _guiTimer = new Timer();
                _guiTimer.Elapsed += GuiTimerOnElapsed;
                _guiTimer.Interval = 150;
                _guiTimer.Enabled = true;
                _guiTimer.AutoReset = false;
            }
        }

        private void GuiTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_guiTimer)
            {
                foreach (var plugin in _plugins.ToArray())
                {
                    if (plugin.IsEditorOpen)
                    {
                        plugin.PluginContext.PluginCommandStub.EditorIdle();
                        plugin.RedrawEditor();
                    }
                }
            }


            _guiTimer.Start();
        }

        private void AudioTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_audioTimer)
            {
                foreach (var plugin in _plugins.ToArray())
                {
                    IdleLoop(plugin.PluginContext, 1);
                }
            }

            _audioTimer.Start();
        }

        /// <summary>
        ///     Returns all found DLLs for a specific directory
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <returns></returns>
        


        public const int BlockSize = 512;
        public const float SampleRate = 44100f;

        private static List<VstPlugin> _plugins = new List<VstPlugin>();

        public bool LoadVST(Plugin vst, int idleLoopCount = 1024)
        {
            throw new Exception("Dont use anymore");
        }

        public bool LoadVst(VstPlugin vst)
        {
            LoadVstInternal(vst);

            if (vst.BackgroundProcessing)
            {
                lock (_audioTimer)
                {
                    lock (_guiTimer)
                    {
                        _plugins.Add(vst);
                    }
                }
            }

            return true;
        }

        public void ReloadPlugin(VstPlugin vst)
        {
            UnloadVst(vst);
            LoadVstInternal(vst);
        }

        private static void LoadVstInternal(VstPlugin vst)
        {
            var hostCommandStub = new NewHostCommandStub();
            hostCommandStub.PluginDll = Path.GetFileName(vst.DllPath);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Loading plugin");
            var ctx = VstPluginContext.Create(vst.DllPath, hostCommandStub);
            ctx.Set("Plugin", vst);

            vst.PluginContext = ctx;
            ctx.Set("PluginPath", vst.DllPath);
            ctx.Set("HostCmdStub", hostCommandStub);
            Debug.WriteLine($"{hostCommandStub.PluginDll}: Opening plugin");
            ctx.PluginCommandStub.Open();

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Set Block Size");
            ctx.PluginCommandStub.SetBlockSize(BlockSize);
            Debug.WriteLine($"{hostCommandStub.PluginDll}: Set Sample Rate");
            ctx.PluginCommandStub.SetSampleRate(SampleRate);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Activating output");
            vst.PluginContext.PluginCommandStub.MainsChanged(true);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: starting to process");
            ctx.PluginCommandStub.StartProcess();
            
            Debug.WriteLine($"{vst.DllFilename}: adding to list");
        }

        public static void IdleLoop(IVstPluginContext ctx, int loops)
        {
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

        private void MIDI(VstPlugin plugin, byte Cmd, byte Val1, byte Val2)
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
            midiData[3] = 0; // Reserved, unused

            var vse = new VstMidiEvent( /*DeltaFrames*/ 0,
                /*NoteLength*/ 0,
                /*NoteOffset*/ 0,
                midiData,
                /*Detune*/ 0,
                /*NoteOffVelocity*/ 127);

            var ve = new VstEvent[1];
            ve[0] = vse;

            plugin.PluginContext.PluginCommandStub.ProcessEvents(ve);
        }

        public void MIDI_CC(VstPlugin plugin, byte Number, byte Value)
        {
            byte Cmd = 0xB0;
            MIDI(plugin, Cmd, Number, Value);
        }

        public void MIDI_NoteOff(VstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x80;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void MIDI_NoteOn(VstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x90;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void UnloadVst(VstPlugin vst)
        {
            lock (_audioTimer)
            {
                lock (_guiTimer)
                {
                    if (vst.BackgroundProcessing)
                    {
                        Debug.WriteLine($"{vst.DllFilename}: removing from list");
                        _plugins.Remove(vst);
                    }


                    if (vst.IsEditorOpen)
                    {
                        Debug.WriteLine($"{vst.DllFilename}: closing editor in shutdown");
                        Application.Current.Dispatcher.Invoke(() => { vst.CloseEditor(); });
                    }

                    Debug.WriteLine($"{vst.DllFilename}: stopping process");
                    vst.PluginContext?.PluginCommandStub.StopProcess();
                    Debug.WriteLine($"{vst.DllFilename}: turning off");
                    vst.PluginContext?.PluginCommandStub.MainsChanged(false);

                    Debug.WriteLine($"{vst.DllFilename}: starting shutdown");
                    vst.PluginContext?.PluginCommandStub.Close();
                    vst.PluginContext = null;
                }
            }
        }

        public void DisposeVST(VstPlugin plugin)
        {
        }
    }
}