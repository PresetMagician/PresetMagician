using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NAudio.Wave;
using System.Threading.Tasks;
using CommonUtils.VSTPlugin;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using MidiVstTest;
using System.Threading;
using System.Timers;
using NVorbis.Ogg;
using System.Diagnostics;
using OggVorbisEncoder;

namespace TestRIFF
{
    class VstHost
    {

        private const int SampleSize = 1024;
        private bool stoppedPlaying = false;
        private bool audioBufferEmpty = false;

        private VST vst = null;
        private VSTStream vstStream = null;

        private string _pluginPath = null;
        private int _sampleRate = 0;
        private int _channels = 0;

        private static System.Timers.Timer aTimer;
        private static System.Timers.Timer aTimer2;

        public VstHost ()
        {
            Process myProcess;
            String pluginPath = @"C:\Program Files\Native Instruments\VSTPlugins 64 bit\V-Station x64.dll";

            var hcs = new HostCommandStub();
            

            VstPluginContext ctx = VstPluginContext.Create(pluginPath, hcs);
            vst = new VST();

            vst.PluginContext = ctx;
            vst.PluginContext.Set("PluginPath", pluginPath);
            vst.PluginContext.Set("HostCmdStub", hcs);
            vst.PluginContext.PluginCommandStub.Open();

            // add custom data to the context
            ctx.Set("PluginPath", pluginPath);

            // actually open the plugin itself
            

            Console.Write("PluginId: ");
            Console.WriteLine(ctx.PluginInfo.PluginID);
            Console.WriteLine(ctx.PluginCommandStub.GetVendorString());

            Console.Write("Programs found: ");
            Console.WriteLine(ctx.PluginInfo.ProgramCount);

            if ((ctx.PluginCommandStub.PluginContext.PluginInfo.Flags & VstPluginFlags.IsSynth) == 0)
            {
                Console.WriteLine("Plugin is an effect, returning");
                return;
            }

            if (ctx.PluginCommandStub.PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)) == VstCanDoResult.No)
            {
                Console.WriteLine("This plugin does not support offline processing.");
                Console.WriteLine("Try use realtime (-play) instead!");
                return;
            }

            for (int i = 1; i < ctx.PluginInfo.ProgramCount; i++)
            {
                ctx.PluginCommandStub.SetProgram(i);
                Console.Write("Processing program: ");
                Console.Write(ctx.PluginCommandStub.GetProgramName());

                //Console.WriteLine(ByteArrayToString(ctx.PluginCommandStub.GetChunk(false)));
                //Console.WriteLine(ByteArrayToString(ctx.PluginCommandStub.GetChunk(true)));
                //Console.WriteLine(ctx.PluginCommandStub.GetChunk(false).Length);

                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] result = sha.ComputeHash(ctx.PluginCommandStub.GetChunk(true));

                Console.WriteLine(ByteArrayToString(result));


                vstStream = new VSTStream();
                vstStream.DoProcess = false;
                   vstStream.pluginContext = ctx;
                vstStream.SetWaveFormat(44100, 2);
                
                

                var buffer2 = new byte[512 * 4];



                vstStream.DoProcess = true;
                int qread;
                int ziV;
                for ( ziV=0;ziV<1024;ziV++) { 
                
                qread = vstStream.Read(buffer2, 0, buffer2.Length);
                }
                vst.MIDI_NoteOn(60, 127);

                
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

                        if (ms.Length > 352800*6)
                        {
                            stoppedPlaying = true;
                        }
                    }

                    // save
                    using (WaveStream ws = new RawSourceWaveStream(ms, vstStream.WaveFormat))
                    {
                        ws.Position = 0;
                        //var stdout = new FileStream(@"C:\Users\Felicia Hummel\Documents\Native Instruments\User Content\VStation\VStation Factory\.previews\"+(99+i).ToString()+ ctx.PluginCommandStub.GetProgramName()+".nksf.ogg", FileMode.Create, FileAccess.Write);
                       
                        WaveFileWriter.CreateWaveFile(@"C:\Users\Felicia Hummel\Documents\Native Instruments\User Content\VStation\VStation Factory\.previews\temp.wav", ws);

                        myProcess = Process.Start(@"C:\Users\Felicia Hummel\Downloads\ffmpeg.exe","-i \"C:\\Users\\Felicia Hummel\\Documents\\Native Instruments\\User Content\\VStation\\VStation Factory\\.previews\\temp.wav\" -acodec libvorbis \"C:\\Users\\Felicia Hummel\\Documents\\Native Instruments\\User Content\\VStation\\VStation Factory\\.previews\\"+(100+i).ToString()+" "+ ctx.PluginCommandStub.GetProgramName()+".nksf.ogg\" -y");
                        myProcess.WaitForExit();

                        //var stdin = new FileStream(@"C:\Users\Felicia Hummel\Documents\Native Instruments\User Content\VStation\VStation Factory\.previews\temp.wav", FileMode.Open, FileAccess.Read);
                        /*
                        var info = VorbisInfo.InitVariableBitRate(2, 44100, 0.1f);
                        // set up our packet->stream encoder
                        var serial = new Random().Next();
                        var oggStream = new OggStream(serial);
                        var headerBuilder = new HeaderPacketBuilder();

                        var comments = new Comments();
                        comments.AddTag("ARTIST", "TEST");

                        var infoPacket = headerBuilder.BuildInfoPacket(info);
                        var commentsPacket = headerBuilder.BuildCommentsPacket(comments);
                        var booksPacket = headerBuilder.BuildBooksPacket(info);

                        oggStream.PacketIn(infoPacket);
                        oggStream.PacketIn(commentsPacket);
                        oggStream.PacketIn(booksPacket);

                        // Flush to force audio data onto its own page per the spec
                        OggPage page;
                        while (oggStream.PageOut(out page, true))
                        {
                            stdout.Write(page.Header, 0, page.Header.Length);
                            stdout.Write(page.Body, 0, page.Body.Length);
                        }
                        var processingState = ProcessingState.Create(info);

            var buffer = new float[info.Channels][];
            buffer[0] = new float[SampleSize];
            buffer[1] = new float[SampleSize];

            var readbuffer = new byte[SampleSize*4];
            while (!oggStream.Finished)
            {
                var bytes = stdin.Read(readbuffer, 0, readbuffer.Length);

                if (bytes == 0)
                {
                    processingState.WriteEndOfStream();
                }
                else
                {
                    var samples = bytes/4;

                    for (var j = 0; j < samples; j++)
                    {
                        // uninterleave samples
                        buffer[0][j] = (short) ((readbuffer[j*4 + 1] << 8) | (0x00ff & readbuffer[j*4]))/32768f;
                        buffer[1][j] = (short) ((readbuffer[j*4 + 3] << 8) | (0x00ff & readbuffer[j*4 + 2]))/32768f;
                    }

                    processingState.WriteData(buffer, samples);
                }

                OggPacket packet;
                while (!oggStream.Finished
                       && processingState.PacketOut(out packet))
                {
                    oggStream.PacketIn(packet);

                    while (!oggStream.Finished
                           && oggStream.PageOut(out page, false))
                    {
                        stdout.Write(page.Header, 0, page.Header.Length);
                        stdout.Write(page.Body, 0, page.Body.Length);
                    }
                }
            }

            stdout.Close();
                        stdin.Close();

                        
                        */

                    }
                }

                // reset the input wave file
                vstStream.DoProcess = false;
                stoppedPlaying = false;

            }

            ctx.PluginCommandStub.Close();
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

    }
}
