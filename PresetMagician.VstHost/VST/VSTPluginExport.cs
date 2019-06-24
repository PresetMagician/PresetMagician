using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Catel.Collections;
using Drachenkatze.PresetMagician.VSTHost.Properties;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using MethodTimer;
using NAudio.Wave;
using PresetMagician.Core.Models;
using PresetMagician.NKS;

namespace PresetMagician.VstHost.VST
{
    public class NKSExport
    {
        public NKSExport(VstHost vstHost)
        {
            VstHost = vstHost;
        }

        public VstHost VstHost { get; }


        public void ExportNKSPreset(PresetExportInfo preset, byte[] data)
        {
            var nksf = new NKSFRiff();


            nksf.kontaktSound.summaryInformation.summaryInformation.vendor = preset.PluginVendor;
            nksf.kontaktSound.summaryInformation.summaryInformation.uuid = preset.PresetGuid;
            nksf.kontaktSound.summaryInformation.summaryInformation.name = preset.PresetName;

            if (preset.PluginType == Plugin.PluginTypes.Instrument)
            {
                nksf.kontaktSound.summaryInformation.summaryInformation.deviceType = "INST";
            }
            else if (preset.PluginType == Plugin.PluginTypes.Effect)
            {
                nksf.kontaktSound.summaryInformation.summaryInformation.deviceType = "FX";
            }
            else
            {
                throw new ArgumentException("Unknown device type");
            }

            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.Add(preset.PluginName);

            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.AddRange(preset.BankPath);

            nksf.kontaktSound.summaryInformation.summaryInformation.Types = preset.Types;
            nksf.kontaktSound.summaryInformation.summaryInformation.Modes = preset.Modes;
            nksf.kontaktSound.summaryInformation.summaryInformation.author = preset.Author;

            nksf.kontaktSound.summaryInformation.summaryInformation.comment =
                preset.Comment + Environment.NewLine + "Generated with PresetMagician";
            nksf.kontaktSound.pluginId.pluginId.VSTMagic = (uint) preset.PluginId;
            nksf.kontaktSound.pluginChunk.PresetData = data;

            if (preset.DefaultControllerAssignments != null)
            {
                nksf.kontaktSound.controllerAssignments.controllerAssignments =
                    preset.DefaultControllerAssignments;
            }

            var outputFilename = preset.GetFullOutputPath();
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));
            var fileStream2 = new FileStream(outputFilename, FileMode.Create);
            nksf.Write(fileStream2);
            fileStream2.Close();
        }


        private void ConvertToOGG(string inputWave, string outputOGG)
        {
            var path = Path.Combine(Path.GetTempPath(), "ffmpeg.exe");

            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, Resources.ffmpeg);
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = path,
                    Arguments = "-i \"" + inputWave + "\" -acodec libvorbis -y \"" + outputOGG + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            Debug.WriteLine("ffmpeg path: " + process.StartInfo.FileName);
            Debug.WriteLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            process.Start();
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        [Time]
        public void ExportPresetAudioPreviewRealtime(RemoteVstPlugin plugin, PresetExportInfo preset, byte[] data,
            int initialDelay)
        {
            var ctx = plugin.PluginContext;

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                throw new EffectsNotSupportedException();
            }

            // check if the plugin supports real time processing
            if (ctx.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)) == VstCanDoResult.Yes)
            {
                throw new NoRealtimeProcessingException();
            }

            ctx.PluginCommandStub.SetChunk(data, false);


            var tempFileName = preset.GetPreviewFilename(true);
            Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));

            var noteOnEvents = new List<(int loop, int offset, byte note)>();
            var noteOffEvents = new List<(int loop, int offset, byte note)>();

            foreach (var note in preset.PreviewNotePlayer.PreviewNotes)
            {
                var onLoop = (double) VstHost.SampleRate * note.Start / VstHost.BlockSize;
                var onOffset = (int) ((onLoop - (int) onLoop) * VstHost.BlockSize);

                noteOnEvents.Add((loop: (int) onLoop, offset: onOffset, note: (byte) (note.NoteNumber + 12)));
                var offLoop = (double) VstHost.SampleRate * (note.Start + note.Duration) / VstHost.BlockSize;
                var offOffset = (int) ((offLoop - (int) offLoop) * VstHost.BlockSize);

                noteOffEvents.Add((loop: (int) offLoop, offset: offOffset, note: (byte) (note.NoteNumber + 12)));
            }

            var hasExportedAudio = false;

            for (var i = 0; i < 10; i++)
            {
                if (DoAudioWaveExport(plugin, tempFileName, noteOnEvents, noteOffEvents, initialDelay,
                    preset.PreviewNotePlayer.MaxDuration))
                {
                    hasExportedAudio = true;
                    break;
                }
            }


            if (hasExportedAudio)
            {
                ConvertToOGG(tempFileName, preset.GetPreviewFilename());
            }
            else
            {
                plugin.Logger.Error("No audio data was returned by the plugin. Most likely it was still loading " +
                                    "the preset data; try to increase the audio preview pre-delay in the plugin " +
                                    "settings and try again");
            }

            File.Delete(tempFileName);
        }

        private bool DoAudioWaveExport(RemoteVstPlugin plugin, string tempFileName,
            List<(int loop, int offset, byte note)> noteOnEvents, List<(int loop, int offset, byte note)> noteOffEvents,
            int initialDelay, int targetLength)
        {
            var dataWritten = false;

            if (targetLength < 1)
            {
                targetLength = 1;
            }

            var ctx = plugin.PluginContext;

            var outputCount = ctx.PluginInfo.AudioOutputCount;
            var inputCount = ctx.PluginInfo.AudioInputCount;

            using (var inputMgr = new VstAudioBufferManager(inputCount, VstHost.BlockSize))
            {
                using (var outputMgr = new VstAudioBufferManager(outputCount, VstHost.BlockSize))
                {
                    var outputBuffers = outputMgr.ToArray();
                    var inputBuffers = inputMgr.ToArray();

                    var p = WaveFormat.CreateIeeeFloatWaveFormat((int) VstHost.SampleRate, 2);
                    var loops = (int) VstHost.SampleRate * targetLength / VstHost.BlockSize;

                    var writer = new WaveFileWriter(tempFileName, p);


                    // Empty buffer
                    int k;

                    for (k = 0; k < initialDelay; k++)
                    {
                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                        Thread.Sleep(10);
                    }

                    for (k = 0; k < loops; k++)
                    {
                        foreach (var i in noteOnEvents)
                        {
                            if (i.loop == k)
                            {
                                VstHost.MIDI_NoteOn(plugin, (byte) i.note, 127, i.offset);
                            }
                        }

                        foreach (var i in noteOffEvents)
                        {
                            if (i.loop == k)
                            {
                                VstHost.MIDI_NoteOff(plugin, (byte) i.note, 127, i.offset);
                            }
                        }

                        if (HasBufferData(outputBuffers))
                        {
                            WriteBuffers(outputBuffers, writer);
                            dataWritten = true;
                        }

                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                    }

                    writer.Close();
                }
            }

            return dataWritten;
        }

        private bool HasBufferData(VstAudioBuffer[] outputBuffers)
        {
            for (var j = 0; j < VstHost.BlockSize; j++)
            {
                var x = 0;
                foreach (var t in outputBuffers)
                {
                    if (x < 2)
                    {
                        if (t[j] != 0)
                        {
                            return true;
                        }
                    }

                    x++;
                }
            }

            return false;
        }

        private void WriteBuffers(VstAudioBuffer[] outputBuffers, WaveFileWriter writer)
        {
            for (var j = 0; j < VstHost.BlockSize; j++)
            {
                var x = 0;
                foreach (var t in outputBuffers)
                {
                    if (x < 2)
                    {
                        writer.WriteSample(t[j]);
                    }

                    x++;
                }
            }
        }
    }
}