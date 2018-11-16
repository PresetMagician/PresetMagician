using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using NAudio.Wave;
using Drachenkatze.PresetMagician.VSTHost;
using Drachenkatze.PresetMagician.VSTHost.Properties;
using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class NoOfflineSupportException : Exception
    {
    }

    public class EffectsNotSupportedException : Exception
    {
    }

    public class VstHost
    {
        private const int SampleSize = 1024;
        private bool stoppedPlaying = false;
        private bool audioBufferEmpty = false;

        private VSTPlugin vst = null;
        private VSTStream vstStream = null;

        private string _pluginPath = null;
        private int _sampleRate = 0;
        private int _channels = 0;

        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer aTimer2;

        public VstHost()
        {
        }

        public ObservableCollection<String> EnumeratePlugins(DirectoryInfo pluginDirectory)
        {
            ObservableCollection<String> vstPlugins = new ObservableCollection<String>();
            foreach (string file in Directory.EnumerateFiles(
    pluginDirectory.FullName, "*.dll", SearchOption.AllDirectories))
            {
                vstPlugins.Add(file);
            }

            return vstPlugins;
        }

        public VSTPlugin LoadVST(VSTPlugin vst)
        {
            HostCommandStub hostCommandStub = new HostCommandStub();

            //hostCommandStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);
            hostCommandStub.Directory = Path.GetDirectoryName(vst.PluginDLLPath);

            try
            {
                VstPluginContext ctx = VstPluginContext.Create(vst.PluginDLLPath, hostCommandStub);

                vst.PluginContext = ctx;
                ctx.Set("PluginPath", vst.PluginDLLPath);
                ctx.Set("HostCmdStub", hostCommandStub);
                ctx.PluginCommandStub.Open();
                vst.PluginContext.PluginCommandStub.MainsChanged(true);
                vst.doCache();
                vst.LoadError = "Loaded.";
            }
            catch (Exception e)
            {
                Debug.WriteLine("load error");
                vst.LoadError = "Could not load plugin. " + e.ToString();
            }

            return vst;
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

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

        public void UnloadVST(VSTPlugin vst)
        {
            //vst.PluginContext.PluginCommandStub.
            //vst.PluginContext.PluginCommandStub.StopProcess();
            //vst.PluginContext.PluginCommandStub.Close();
            Debug.WriteLine("Trying to dispose plugin2");

            if (vst.PluginContext != null)
            {
                //vst.PluginContext.Dispose();
            }
        }
           
        public void ExportPresetNKSF(VSTPreset preset)
        {
            NKSFRiff nksf = new NKSFRiff();
            Guid guid = Guid.NewGuid();

            preset.VstPlugin.PluginContext.PluginCommandStub.SetProgram(preset.ProgramNumber);
            nksf.kontaktSound.summaryInformation.summaryInformation.vendor = preset.VstPlugin.PluginVendor;
            nksf.kontaktSound.summaryInformation.summaryInformation.uuid = guid;
            nksf.kontaktSound.summaryInformation.summaryInformation.name = preset.PresetName;
            nksf.kontaktSound.summaryInformation.summaryInformation.deviceType = "INST";
            nksf.kontaktSound.summaryInformation.summaryInformation.bankChain.Add(preset.VstPlugin.PluginName);
            nksf.kontaktSound.pluginId.pluginId.VSTMagic = preset.VstPlugin.PluginContext.PluginInfo.PluginID;
            nksf.kontaktSound.pluginChunk.PresetData = preset.PresetData;
            Debug.WriteLine("chunk size " + preset.PresetData.Length);

            String outputFilename = Path.Combine(getUserContentDirectory(preset), preset.NKSFPresetName + ".nksf");
            var fileStream2 = new FileStream(outputFilename, FileMode.Create);
            nksf.Write(fileStream2);
            fileStream2.Close();
        }

        public String getUserContentDirectory (VSTPreset preset)
        {
            String userContentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Native Instruments\User Content");
            String bankDirectory = Path.Combine(userContentDirectory, preset.NKSFPluginName, preset.NKSFBankName);
            Directory.CreateDirectory(bankDirectory);
            return bankDirectory;
        }
        public void ExportPresetAudioPreview(VSTPreset preset)
        {
            VSTPlugin vst = preset.VstPlugin;
            if (!vst.IsOpened) { 
            vst.PluginContext.PluginCommandStub.Open();
            }
            vst.IsOpened = true;
            VstPluginContext ctx = vst.PluginContext;

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                throw new EffectsNotSupportedException();
            }

            if (ctx.PluginCommandStub.PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)) == VstCanDoResult.No)
            {
                throw new NoOfflineSupportException();
            }

            ctx.PluginCommandStub.SetChunk(preset.PresetData, false);

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

                    string path = Path.Combine(Path.GetTempPath(), "ffmpeg.exe");
                    File.WriteAllBytes(path, Resources.ffmpeg);

                    String bankDirectory = getUserContentDirectory(preset);

                    String previewDirectory = Path.Combine(bankDirectory, ".previews");

                    Directory.CreateDirectory(previewDirectory);

                    String previewOutputFilename = Path.Combine(previewDirectory, preset.NKSFPresetName + ".nksf.ogg");

                    var process = new Process
                    {
                        StartInfo =
                          {
                              FileName = path,
                              Arguments = "-i " + tempFileName + " -acodec libvorbis -y \""+previewOutputFilename+"\"",
                              CreateNoWindow = true,
                              WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
                    };

                    Debug.WriteLine("ffmpeg path: " + process.StartInfo.FileName);
                    Debug.WriteLine("ffmpeg arguments: " + process.StartInfo.Arguments);

                    /*process.Start();
                    process.WaitForExit();
                    process.Close();
                    process.Dispose();*/
                }

                vstStream.DoProcess = false;
                stoppedPlaying = false;
            }

            //vst.PluginContext.PluginCommandStub.Close();
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
            if (UtilityAudio.PlaybackDevice != null)
            {
                UtilityAudio.StartStreamingToDisk();
                Console.WriteLine("Started streaming to disk ...");
            }
            Console.WriteLine("Vst Plugin Started playing ...");
            stoppedPlaying = false;
        }

        private void vst_PlayingStopped(object sender, EventArgs e)
        {
            if (UtilityAudio.PlaybackDevice != null)
            {
                UtilityAudio.VstStream.DoProcess = false;
                UtilityAudio.StopStreamingToDisk();
                Console.WriteLine("Stopped streaming to disk ...");
            }
            Console.WriteLine("Vst Plugin Stopped playing ...");
            stoppedPlaying = true;
        }

        private void vst_ProcessCalled(object sender, VSTStreamEventArgs e)
        {
            if (e.MaxL == 0 && e.MaxR == 0)
            {
                audioBufferEmpty = true;
            }
            else
            {
                audioBufferEmpty = false;
            }
        }

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
    }
}