using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;
using PresetMagician.Core.Services;
using RtMidi.Core.Devices;
using RtMidi.Core.Messages;

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

        private readonly object _guiLock = new object();
        private readonly object _audioLock = new object();
        private WasapiOut _outputDevice;
        private VSTStream _vstWaveProvider;
        private RemoteVstPlugin _midiTarget;
        private List<IMidiInputDevice> _midiInputDevices = new List<IMidiInputDevice>();


        public VstHost(bool useTimer = false)
        {
            if (useTimer)
            {
                _audioTimer = new Timer();
                _audioTimer.Elapsed += AudioTimerOnElapsed;
                _audioTimer.Interval = 50;
                _audioTimer.Enabled = false;
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
            lock (_guiLock)
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
            lock (_audioLock)
            {
                foreach (var plugin in _plugins.ToArray())
                {
                    var ctx = plugin.PluginContext.Find<NewHostCommandStub>("HostCmdStub");
                    ctx.SetProcessLevel(VstProcessLevels.Realtime);
                    IdleLoop(plugin.PluginContext, 1);
                    ctx.SetProcessLevel(VstProcessLevels.User);
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

        public void LoadVst(RemoteVstPlugin remoteVst, bool debug = false)
        {
            LoadVstInternal(remoteVst, debug);

            if (remoteVst.BackgroundProcessing)
            {
                lock (_audioLock)
                {
                    lock (_guiLock)
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

        private static void LoadVstInternal(RemoteVstPlugin remoteVst, bool debug = false)
        {
            var hostCommandStub = new NewHostCommandStub(remoteVst.Logger);
            hostCommandStub.PluginDll = Path.GetFileName(remoteVst.DllPath);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Loading plugin");


            var ctx = VstPluginContext.Create(remoteVst.DllPath, hostCommandStub);
            ctx.Set("Plugin", remoteVst);

            remoteVst.PluginContext = ctx;
            ctx.Set("PluginPath", remoteVst.DllPath);
            ctx.Set("HostCmdStub", hostCommandStub);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Opening plugin");

            ctx.PluginCommandStub.Open();

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Setting Sample Rate {SampleRate}");


            ctx.PluginCommandStub.SetSampleRate(SampleRate);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Setting Block Size {BlockSize}");

            ctx.PluginCommandStub.SetBlockSize(BlockSize);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Setting 32 bit precision");

            ctx.PluginCommandStub.SetProcessPrecision(VstProcessPrecision.Process32);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Activating output");


            remoteVst.PluginContext.PluginCommandStub.MainsChanged(true);

            remoteVst.Logger.Debug($"{hostCommandStub.PluginDll}: Start Processing");

            ctx.PluginCommandStub.StartProcess();

            remoteVst.IsLoaded = true;
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

        private void MIDI(RemoteVstPlugin plugin, byte Cmd, byte Val1, byte Val2, int deltaFrames = 0)
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

            var vse = new VstMidiEvent(deltaFrames,
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

        public void MIDI_NoteOff(RemoteVstPlugin plugin, byte Note, byte Velocity, int deltaFrames = 0)
        {
            byte Cmd = 0x80;
            MIDI(plugin, Cmd, Note, Velocity, deltaFrames);
        }

        public void MIDI_NoteOn(RemoteVstPlugin plugin, byte Note, byte Velocity, int deltaFrames = 0)
        {
            byte Cmd = 0x90;
            MIDI(plugin, Cmd, Note, Velocity, deltaFrames);
        }

        public void UnloadVst(RemoteVstPlugin remoteVst)
        {
            lock (_audioLock)
            {
                lock (_guiLock)
                {
                    if (remoteVst.BackgroundProcessing)
                    {
                        Debug.WriteLine($"{remoteVst.DllFilename}: removing from list");
                        _plugins.Remove(remoteVst);
                    }

                    UnpatchPluginFromAudioOutput();
                    UnpatchPluginFromMidiInput();

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
                    remoteVst.IsLoaded = false;
                }
            }
        }

        public void PatchPluginToAudioOutput(RemoteVstPlugin plugin, AudioOutputDevice device, int latency)
        {
            var audioService = new AudioService();
            var ep = audioService.GetAudioEndpoint(device);

            if (ep == null)
            {
                throw new Exception(
                    $"Unable to open audio device {device.Name} because the device (currently) doesn't exist");
            }

            UnpatchPluginFromAudioOutput();


            _outputDevice = new WasapiOut(ep, AudioClientShareMode.Shared, true, latency);

            _vstWaveProvider = new VSTStream();
            _vstWaveProvider.pluginContext = plugin.PluginContext;
            _vstWaveProvider.SetWaveFormat(_outputDevice.OutputWaveFormat.SampleRate,
                _outputDevice.OutputWaveFormat.Channels);
            _outputDevice.Init(_vstWaveProvider);
            _outputDevice.Play();
        }

        public void PerformIdleLoop(RemoteVstPlugin plugin, int loops)
        {
            IdleLoop(plugin.PluginContext, loops);
        }

        public void PatchPluginToMidiInput(RemoteVstPlugin plugin, MidiInputDevice device)
        {
            _midiTarget = plugin;
            var midiService = new MidiService();
            var midiInput = midiService.GetMidiEndpoint(device);

            if (midiInput == null)
            {
                throw new Exception(
                    $"Unable to open MIDI device {device.Name} because the device  (currently) doesn't exists");
            }

            midiInput.NoteOn += MidiInputOnNoteOn;
            midiInput.NoteOff += MidiInputOnNoteOff;
            midiInput.Open();

            _midiInputDevices.Add(midiInput);
        }

        public void UnpatchPluginFromMidiInput()
        {
            foreach (var inputDevice in _midiInputDevices)
            {
                inputDevice.Close();
                inputDevice.Dispose();
            }

            _midiTarget = null;
        }

        private void MidiInputOnNoteOff(IMidiInputDevice sender, in NoteOffMessage msg)
        {
            MIDI_NoteOff(_midiTarget, (byte) msg.Key, (byte) msg.Velocity);
        }

        private void MidiInputOnNoteOn(IMidiInputDevice sender, in NoteOnMessage msg)
        {
            MIDI_NoteOn(_midiTarget, (byte) msg.Key, (byte) msg.Velocity);
        }

        public void UnpatchPluginFromAudioOutput()
        {
            if (_outputDevice == null)
            {
                return;
            }

            _outputDevice.Stop();
            _vstWaveProvider = null;
            _outputDevice = null;
        }

        public void DisposeVST(RemoteVstPlugin plugin)
        {
        }
    }
}