using Drachenkatze.PresetMagician.NKSF.NKSF;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class VSTPluginExport
    {
        private const int SampleSize = 1024;
        private bool stoppedPlaying = false;
        private VSTStream vstStream = null;

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        /// <summary>
        ///     We cheat on the WAV header; we just bypass the header and never
        ///     verify that it matches 16bit/stereo/44.1kHz.This is just an
        ///     example, after all.
        /// </summary>
        private static void StripWavHeader(BinaryReader stdin)
        {
            var tempBuffer = new byte[6];
            for (var i = 0; (i < 30) && (stdin.Read(tempBuffer, 0, 2) > 0); i++)
                if ((tempBuffer[0] == 'd') && (tempBuffer[1] == 'a'))
                {
                    stdin.Read(tempBuffer, 0, 6);
                    break;
                }
        }

        private void vst_PlayingStarted(object sender, EventArgs e)
        {
            stoppedPlaying = false;
        }

        private void vst_PlayingStopped(object sender, EventArgs e)
        {
            stoppedPlaying = true;
        }

        public void ExportPresetNKSF(VSTPlugin plugin, VSTPreset preset)
        {
            VSTPlugin vst = plugin;
            vst.PluginContext.PluginCommandStub.Open();

            NKSFRiff nksf = new NKSFRiff();
            Guid guid = Guid.NewGuid();

            nksf.kontaktSound.summaryInformation.summaryInformation.vendor = plugin.PluginVendor;
            nksf.kontaktSound.summaryInformation.summaryInformation.uuid = guid;
            nksf.kontaktSound.summaryInformation.summaryInformation.name = preset.PresetName;
            nksf.kontaktSound.summaryInformation.summaryInformation.deviceType = "INST";
            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.Add(plugin.PluginName);
            nksf.kontaktSound.pluginId.pluginId.VSTMagic = plugin.PluginContext.PluginInfo.PluginID;
            nksf.kontaktSound.pluginChunk.PresetData = preset.PresetData;
            Debug.WriteLine("chunk size " + preset.PresetData.Length);

            String outputFilename = Path.Combine(getUserContentDirectory(preset), preset.NKSFPresetName + ".nksf");
            var fileStream2 = new FileStream(outputFilename, FileMode.Create);
            nksf.Write(fileStream2);
            fileStream2.Close();
        }

        public String getUserContentDirectory(VSTPreset preset)
        {
            String userContentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Native Instruments\User Content");
            String bankDirectory = Path.Combine(userContentDirectory, preset.NKSFPluginName, preset.NKSFBankName);
            Directory.CreateDirectory(bankDirectory);
            return bankDirectory;
        }

        public void ExportPresetAudioPreviewOffline(VSTPlugin plugin, VSTPreset preset)
        {
            VSTPlugin vst = plugin;
            vst.PluginContext.PluginCommandStub.Open();

            VstPluginContext ctx = vst.PluginContext;

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                throw new EffectsNotSupportedException();
            }

            if (ctx.PluginCommandStub.PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)) == VstCanDoResult.No)
            {
                throw new NoOfflineSupportException();
            }

            ctx.PluginCommandStub.BeginSetProgram();
            ctx.PluginCommandStub.SetChunk(preset.PresetData, VSTPlugin.PresetChunk_UseCurrentProgram);
            ctx.PluginCommandStub.EndSetProgram();
            ctx.PluginCommandStub.SetProgram(preset.ProgramNumber);
            vstStream = new VSTStream();
            vstStream.DoProcess = false;
            vstStream.pluginContext = ctx;
            vstStream.SetWaveFormat(44100, 2);

            var buffer2 = new byte[512 * 4];

            Debug.WriteLine("PresetExport: Emptying buffer");
            vstStream.DoProcess = true;
            int qread;

            // Force the buffer to be empty
            for (int q = 0; q < 1024; q++)
            {
                qread = vstStream.Read(buffer2, 0, buffer2.Length);
            }
            vst.MIDI_NoteOn((byte)preset.PreviewNote.NoteNumber, 127);

            using (var ms = new MemoryStream())
            {
                // keep on reading until it stops playing.
                while (!stoppedPlaying)
                {
                    int read = vstStream.Read(buffer2, 0, buffer2.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    ms.Write(buffer2, 0, read);

                    if (ms.Length > 352800)
                    {
                        vst.MIDI_NoteOff(60, 127);
                    }

                    if (ms.Length > 352800 * 6)
                    {
                        stoppedPlaying = true;
                    }
                }

                // save
                using (WaveStream ws = new RawSourceWaveStream(ms, vstStream.WaveFormat))
                {
                    ws.Position = 0;

                    String tempFileName = Path.GetTempFileName();

                    WaveFileWriter.CreateWaveFile(tempFileName, ws);

                    ConvertToOGG(tempFileName, GetPreviewFilename(preset));
                }

                vstStream.DoProcess = false;
                stoppedPlaying = false;
            }

            //vst.PluginContext.PluginCommandStub.Close();
        }

        private string GetPreviewFilename(VSTPreset vstPreset)
        {
            String bankDirectory = getUserContentDirectory(vstPreset);

            String previewDirectory = Path.Combine(bankDirectory, ".previews");

            Directory.CreateDirectory(previewDirectory);

            return Path.Combine(previewDirectory, vstPreset.NKSFPresetName + ".nksf.ogg");
        }

        private void ConvertToOGG(String inputWave, String outputOGG)
        {
            string path = Path.Combine(Path.GetTempPath(), "ffmpeg.exe");
            File.WriteAllBytes(path, Properties.Resources.ffmpeg);

            var process = new Process
            {
                StartInfo =
                          {
                              FileName = path,
                              Arguments = "-i \"" + inputWave + "\" -acodec libvorbis -y \""+outputOGG+"\"",
                              CreateNoWindow = true,
                              WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };

            Debug.WriteLine("ffmpeg path: " + process.StartInfo.FileName);
            Debug.WriteLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            process.Start();
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        public bool ExportPresetAudioPreviewRealtime(VSTPlugin plugin, VSTPreset preset)
        {
            VSTPlugin vst = plugin;

            VstPluginContext ctx = vst.PluginContext;

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                throw new EffectsNotSupportedException();
            }

            // check if the plugin supports real time proccesing
            if (ctx.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)) == VstCanDoResult.Yes)
            {
                throw new NoRealtimeProcessingException();
            }

            ctx.PluginCommandStub.BeginSetProgram();
            ctx.PluginCommandStub.SetChunk(preset.PresetData, VSTPlugin.PresetChunk_UseCurrentProgram);
            ctx.PluginCommandStub.EndSetProgram();
            ctx.PluginCommandStub.SetProgram(preset.ProgramNumber);

            int outputCount = ctx.PluginInfo.AudioOutputCount;
            int inputCount = ctx.PluginInfo.AudioInputCount;
            int blockSize = 1024;

            String tempFileName = Path.GetTempFileName();
            using (VstAudioBufferManager inputMgr = new VstAudioBufferManager(inputCount, blockSize))
            {
                using (VstAudioBufferManager outputMgr = new VstAudioBufferManager(outputCount, blockSize))
                {
                    ctx.PluginCommandStub.SetBlockSize(blockSize);
                    ctx.PluginCommandStub.SetSampleRate(44100f);

                    VstAudioBuffer[] outputBuffers = outputMgr.ToArray();
                    VstAudioBuffer[] inputBuffers = inputMgr.ToArray();

                    var p = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
                    int targetLength = 6;
                    int noteOffSecond = 1;
                    int loops = (44100 * targetLength) / blockSize;
                    int noteOffLoop = (44100 * noteOffSecond) / blockSize;

                    WaveFileWriter writer = new WaveFileWriter(tempFileName, p);

                    ctx.PluginCommandStub.MainsChanged(true);

                    ctx.PluginCommandStub.StartProcess();
                    // Empty buffer
                    int k;

                    for (k = 0; k < 1024; k++)
                    {
                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                    }

                    vst.MIDI_NoteOn((byte)preset.PreviewNote.NoteNumber, 127);
                    for (k = 0; k < loops; k++)
                    {
                        if (k == noteOffLoop)
                        {
                            vst.MIDI_NoteOff(60, 127);
                        }
                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                        for (int j = 0; j < blockSize; j++)
                        {
                            for (int i = 0; i < outputBuffers.Length; i++)
                            {
                                writer.WriteSample(outputBuffers[i][j]);
                            }
                        }
                    }

                    ctx.PluginCommandStub.StopProcess();
                    ctx.PluginCommandStub.MainsChanged(false);

                    writer.Close();
                }
            }

            stoppedPlaying = false;

            ConvertToOGG(tempFileName, GetPreviewFilename(preset));

            return true;
        }
    }
}