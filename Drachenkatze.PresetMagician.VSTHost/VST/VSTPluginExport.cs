using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VSTHost.Properties;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using MethodTimer;
using NAudio.Wave;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class VstPluginExport
    {
        private const int SampleSize = 1024;
        private bool stoppedPlaying;
        private VSTStream vstStream;

        public VstPluginExport(VstHost vstHost)
        {
            VstHost = vstHost;
        }

        public VstHost VstHost { get; }

        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private byte[] StringToByteArray(string str)
        {
            var enc = new ASCIIEncoding();
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
            for (var i = 0; i < 30 && stdin.Read(tempBuffer, 0, 2) > 0; i++)
            {
                if (tempBuffer[0] == 'd' && tempBuffer[1] == 'a')
                {
                    stdin.Read(tempBuffer, 0, 6);
                    break;
                }
            }
        }

        private void vst_PlayingStarted(object sender, System.EventArgs e)
        {
            stoppedPlaying = false;
        }

        private void vst_PlayingStopped(object sender, System.EventArgs e)
        {
            stoppedPlaying = true;
        }

        public void ExportPresetNKSF(IVstPlugin plugin, IPreset preset)
        {
            var vst = plugin;
            vst.PluginContext.PluginCommandStub.Open();

            var nksf = new NKSFRiff();
            var guid = Guid.NewGuid();

            nksf.kontaktSound.summaryInformation.summaryInformation.vendor = plugin.PluginVendor;
            nksf.kontaktSound.summaryInformation.summaryInformation.uuid = guid;
            nksf.kontaktSound.summaryInformation.summaryInformation.name = preset.PresetName;
            nksf.kontaktSound.summaryInformation.summaryInformation.deviceType = "INST";
            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.Add(plugin.PluginName);
            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.Add(preset.PresetBank.BankPath);
            nksf.kontaktSound.pluginId.pluginId.VSTMagic = plugin.PluginContext.PluginInfo.PluginID;
            nksf.kontaktSound.pluginChunk.PresetData = preset.PresetData;

            var outputFilename = Path.Combine(getUserContentDirectory(preset),
                getNKSFPresetName(preset.PresetName) + ".nksf");
            var fileStream2 = new FileStream(outputFilename, FileMode.Create);
            nksf.Write(fileStream2);
            fileStream2.Close();
        }

        public string getUserContentDirectory(IPreset preset)
        {
            var userContentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Native Instruments\User Content");

            var bankDirectory = Path.Combine(userContentDirectory, getNKSFPluginName(preset.PluginName),
                GetNKSFBankName(preset.PresetBank.BankName));
            Directory.CreateDirectory(bankDirectory);
            return bankDirectory;
        }

        public string getNKSFPluginName(string pluginName)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                pluginName = pluginName.Replace(c, '_');
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                pluginName = pluginName.Replace(c, '_');
            }

            return pluginName;
        }

        public string getNKSFPresetName(string presetName)
        {
            // Returns the sanitized preset name

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                presetName = presetName.Replace(c, '_');
            }

            foreach (var c in Path.GetInvalidPathChars())
            {
                presetName = presetName.Replace(c, '_');
            }

            return presetName;
        }

      

        private string GetPreviewFilename(IPreset vstPreset)
        {
            var bankDirectory = getUserContentDirectory(vstPreset);

            var previewDirectory = Path.Combine(bankDirectory, ".previews");

            Directory.CreateDirectory(previewDirectory);

            return Path.Combine(previewDirectory,
                getNKSFPresetName(vstPreset.PresetName));
        }

        public string GetNKSFBankName(string bankName)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                bankName = bankName.Replace(c, '_');
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                bankName = bankName.Replace(c, '_');
            }

            return bankName;
        }

        private void ConvertToOGG(string inputWave, string outputOGG)
        {
            var path = Path.Combine(Path.GetTempPath(), "ffmpeg.exe");
            File.WriteAllBytes(path, Resources.ffmpeg);

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

            File.Delete(path);
        }

        [Time]
        public bool ExportPresetAudioPreviewRealtime(IVstPlugin plugin, IPreset preset)
        {
            var vst = plugin;

            var ctx = vst.PluginContext;

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                throw new EffectsNotSupportedException();
            }

            // check if the plugin supports real time proccesing
            if (ctx.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)) == VstCanDoResult.Yes)
            {
                throw new NoRealtimeProcessingException();
            }

            //ctx.PluginCommandStub.BeginSetProgram();
            Debug.WriteLine(ctx.PluginCommandStub.SetChunk(preset.PresetData, VSTPlugin.PresetChunk_UseCurrentProgram));
            //ctx.PluginCommandStub.EndSetProgram();
            //ctx.PluginCommandStub.SetProgram(preset.ProgramNumber);

            var outputCount = ctx.PluginInfo.AudioOutputCount;
            var inputCount = ctx.PluginInfo.AudioInputCount;
            var blockSize = 512;

            var tempFileName = GetPreviewFilename(preset)+".nksf.wav";

            using (var inputMgr = new VstAudioBufferManager(inputCount, blockSize))
            {
                using (var outputMgr = new VstAudioBufferManager(outputCount, blockSize))
                {
                    ctx.PluginCommandStub.SetBlockSize(blockSize);
                    ctx.PluginCommandStub.SetSampleRate(44100f);

                    var outputBuffers = outputMgr.ToArray();
                    var inputBuffers = inputMgr.ToArray();

                    var p = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
                    var targetLength = 6;
                    var noteOffSecond = 1;
                    var loops = 44100 * targetLength / blockSize;
                    var noteOffLoop = 44100 * noteOffSecond / blockSize;

                    var writer = new WaveFileWriter(tempFileName, p);

                    ctx.PluginCommandStub.MainsChanged(true);

                    ctx.PluginCommandStub.StartProcess();
                    // Empty buffer
                    int k;

                    for (k = 0; k < 512; k++)
                    {
                        //ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                    }

                    VstHost.MIDI_NoteOn(vst, (byte) preset.PreviewNote.NoteNumber, 127);
                    for (k = 0; k < loops; k++)
                    {
                        if (k == noteOffLoop)
                        {
                            VstHost.MIDI_NoteOff(vst, 60, 127);
                        }

                        ctx.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                        for (var j = 0; j < blockSize; j++)
                        {
                            for (var i = 0; i < outputBuffers.Length; i++)
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

            ConvertToOGG(tempFileName, GetPreviewFilename(preset)+".nksf.ogg");
            File.Delete(tempFileName);
            return true;
        }
    }
}