using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using Drachenkatze.PresetMagician.VSTHost;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;

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

    public class VstHost
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

        private static List<RemoteVstPlugin> _plugins = new List<RemoteVstPlugin>();

        public void LoadVst(RemoteVstPlugin remoteVst)
        {
            LoadVstInternal(remoteVst);

            if (remoteVst.BackgroundProcessing)
            {
                lock (_audioTimer)
                {
                    lock (_guiTimer)
                    {
                        _plugins.Add(remoteVst);
                    }
                }
            }
        }

        public void ReloadPlugin(RemoteVstPlugin remoteVst)
        {
            UnloadVst(remoteVst);
            LoadVstInternal(remoteVst);
        }

        private static void LoadVstInternal(RemoteVstPlugin remoteVst)
        {
            var hostCommandStub = new NewHostCommandStub();
            hostCommandStub.PluginDll = Path.GetFileName(remoteVst.DllPath);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Loading plugin");
            var ctx = VstPluginContext.Create(remoteVst.DllPath, hostCommandStub);
            ctx.Set("Plugin", remoteVst);

            remoteVst.PluginContext = ctx;
            ctx.Set("PluginPath", remoteVst.DllPath);
            ctx.Set("HostCmdStub", hostCommandStub);
            Debug.WriteLine($"{hostCommandStub.PluginDll}: Opening plugin");
            ctx.PluginCommandStub.Open();

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Set Block Size");
            ctx.PluginCommandStub.SetBlockSize(BlockSize);
            Debug.WriteLine($"{hostCommandStub.PluginDll}: Set Sample Rate");
            ctx.PluginCommandStub.SetSampleRate(SampleRate);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: Activating output");
            remoteVst.PluginContext.PluginCommandStub.MainsChanged(true);

            Debug.WriteLine($"{hostCommandStub.PluginDll}: starting to process");
            ctx.PluginCommandStub.StartProcess();

            Debug.WriteLine($"{remoteVst.DllFilename}: adding to list");
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

        private void MIDI(RemoteVstPlugin plugin, byte Cmd, byte Val1, byte Val2)
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

        public void MIDI_CC(RemoteVstPlugin plugin, byte Number, byte Value)
        {
            byte Cmd = 0xB0;
            MIDI(plugin, Cmd, Number, Value);
        }

        public void MIDI_NoteOff(RemoteVstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x80;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void MIDI_NoteOn(RemoteVstPlugin plugin, byte Note, byte Velocity)
        {
            byte Cmd = 0x90;
            MIDI(plugin, Cmd, Note, Velocity);
        }

        public void UnloadVst(RemoteVstPlugin remoteVst)
        {
            lock (_audioTimer)
            {
                lock (_guiTimer)
                {
                    if (remoteVst.BackgroundProcessing)
                    {
                        Debug.WriteLine($"{remoteVst.DllFilename}: removing from list");
                        _plugins.Remove(remoteVst);
                    }


                    if (remoteVst.IsEditorOpen)
                    {
                        Debug.WriteLine($"{remoteVst.DllFilename}: closing editor in shutdown");
                        Application.Current.Dispatcher.Invoke(() => { remoteVst.CloseEditor(); });
                    }

                    Debug.WriteLine($"{remoteVst.DllFilename}: stopping process");
                    remoteVst.PluginContext?.PluginCommandStub.StopProcess();
                    Debug.WriteLine($"{remoteVst.DllFilename}: turning off");
                    remoteVst.PluginContext?.PluginCommandStub.MainsChanged(false);

                    Debug.WriteLine($"{remoteVst.DllFilename}: starting shutdown");
                    remoteVst.PluginContext?.PluginCommandStub.Close();
                    remoteVst.PluginContext = null;
                }
            }
        }

        public void DisposeVST(RemoteVstPlugin plugin)
        {
        }
    }
}