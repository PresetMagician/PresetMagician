using System;
using NAudio.Wave;
using Jacobi.Vst.Interop.Host;
using Jacobi.Vst.Core;

// Copied from the microDRUM project
// https://github.com/microDRUM
// I think it is created by massimo.bernava@gmail.com
// Modified by perivar@nerseth.com to support processing audio files
namespace CommonUtils.VSTPlugin
{
	public class VSTStreamEventArgs : EventArgs
	{
		public float MaxL = float.MinValue;
		public float MaxR = float.MaxValue;

		public VSTStreamEventArgs(float maxL, float maxR)
		{
			MaxL = maxL;
			MaxR = maxR;
		}
	}
	
	public class VSTStream : WaveStream
	{
		public VstPluginContext pluginContext = null;
		
		// event handlers
		public event EventHandler<VSTStreamEventArgs> ProcessCalled;
		public event EventHandler PlayingStopped;
		public event EventHandler PlayingStarted;
		
		private int BlockSize = 0;
		
		VstAudioBuffer[] inputBuffers;
		VstAudioBuffer[] outputBuffers;
		
		float[] input;
		float[] output;

		private WaveChannel32 wavStream;
		private WaveFileReader wavFileReader;
		
		private int foundSilenceCounter = 0;

		public bool DoProcess = true;
		
		public void SetInputWave(string waveInputFilePath, float volume) {
			// 4 bytes per sample (32 bit)
			this.wavFileReader = new WaveFileReader(waveInputFilePath);
			this.wavStream = new WaveChannel32(this.wavFileReader);
			
			// Leave headroom for plugin processing (0.20 seems OK, any higher causes digital clipping)
			this.wavStream.Volume = volume;
		}
		
		public void DisposeInputWave() {
			if (wavStream != null) {
				this.wavStream.Dispose();
				this.wavStream = null;
			}
			this.wavFileReader.Dispose();
			this.wavFileReader = null;
		}

		public new void Dispose() {
			DisposeInputWave();
			base.Dispose();
		}
		
		private void RaiseProcessCalled(float maxL, float maxR)
		{
			EventHandler<VSTStreamEventArgs> handler = ProcessCalled;

			if (handler != null)
			{
				handler(this, new VSTStreamEventArgs(maxL, maxR));
			}
		}

		private void RaisePlayingStarted()
		{
			if (PlayingStarted != null)
			{
				PlayingStarted(this, EventArgs.Empty);
			}
		}

		private void RaisePlayingStopped()
		{
			if (PlayingStopped != null)
			{
				PlayingStopped(this, EventArgs.Empty);
			}
		}
		
		private void UpdateBlockSize(int blockSize)
		{
			BlockSize = blockSize;

			int inputCount = pluginContext.PluginInfo.AudioInputCount;
			int outputCount = pluginContext.PluginInfo.AudioOutputCount;

			var inputMgr = new VstAudioBufferManager(inputCount, blockSize);
			var outputMgr = new VstAudioBufferManager(outputCount, blockSize);

			pluginContext.PluginCommandStub.SetBlockSize(blockSize);
			pluginContext.PluginCommandStub.SetSampleRate(WaveFormat.SampleRate);
			pluginContext.PluginCommandStub.SetProcessPrecision(VstProcessPrecision.Process32);

			inputBuffers = inputMgr.ToArray();
			outputBuffers = outputMgr.ToArray();

			input = new float[WaveFormat.Channels * blockSize];
			output = new float[WaveFormat.Channels * blockSize];
		}

		private float[] ProcessReplace(int blockSize)
		{
			lock (this) {
				if (blockSize != BlockSize) UpdateBlockSize(blockSize);
				
				// check if we are processing a wavestream (VST) or if this is audio outputting only (VSTi)
				if (wavStream != null) {
					if (wavStream.Position == 0) {
						RaisePlayingStarted();
					}
					
					int sampleCount = blockSize*2;
					int sampleCountx4 = sampleCount * 4;
					int loopSize = sampleCount / WaveFormat.Channels;
					
					// Convert byte array into float array and store in Vst Buffers
					// naudio reads an buffer of interlaced float's
					// must take every 4th byte and convert to float
					// Vst.Net audio buffer format (-1 to 1 floats).
					var naudioBuf = new byte[blockSize * WaveFormat.Channels * 4];
					int bytesRead = wavStream.Read(naudioBuf, 0, sampleCountx4);
					
					// populate the inputbuffers with the incoming wave stream
					// TODO: do not use unsafe - but like this http://vstnet.codeplex.com/discussions/246206 ?
					// this whole section is modelled after http://vstnet.codeplex.com/discussions/228692
					unsafe
					{
						fixed (byte* byteBuf = &naudioBuf[0])
						{
							float* floatBuf = (float*)byteBuf;
							int j = 0;
							for (int i = 0; i < loopSize; i++)
							{
								inputBuffers[0][i] = *(floatBuf + j);
								j++;
								inputBuffers[1][i] = *(floatBuf + j);
								j++;
							}
						}
					}
				}
				
				try
				{
					//pluginContext.PluginCommandStub.MainsChanged(true);
					pluginContext.PluginCommandStub.StartProcess();
					pluginContext.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
					pluginContext.PluginCommandStub.StopProcess();
					//pluginContext.PluginCommandStub.MainsChanged(false);
				}
				catch (Exception ex)
				{
					Console.Out.WriteLine(ex.Message);
				}

				int indexOutput = 0;

				float maxL = float.MinValue;
				float maxR = float.MinValue;

				for (int j = 0; j < BlockSize; j++)
				{
					output[indexOutput] = outputBuffers[0][j];
					output[indexOutput + 1] = outputBuffers[1][j];

					maxL = Math.Max(maxL, output[indexOutput]);
					maxR = Math.Max(maxR, output[indexOutput + 1]);
					indexOutput += 2;
				}

				// try to find when processing input file has reached
				// zero volume level
				float almostZero = 0.0000001f;
				if (maxL < almostZero && maxR < almostZero) {
					//Console.Out.Write("-");
					
					// don't stop until we have x consequetive silence calls after each other
					if (foundSilenceCounter >= 5) {
						if (wavStream != null && wavStream.CurrentTime >= wavStream.TotalTime) {
							RaisePlayingStopped();
						}
					} else {
						foundSilenceCounter++;
					}
				} else {
					foundSilenceCounter = 0;
					//Console.Out.Write(".");
				}
				RaiseProcessCalled(maxL, maxR);
			}
			return output;
		}

		public int Read(float[] buffer, int offset, int sampleCount)
		{
			if (!DoProcess) return 0;
			
			// CALL VST PROCESS HERE WITH BLOCK SIZE OF sampleCount
			float[] tempBuffer = ProcessReplace(sampleCount / 2);

			// Copying Vst buffer inside Audio buffer, no conversion needed for WaveProvider32
			for (int i = 0; i < sampleCount; i++)
				buffer[i + offset] = tempBuffer[i];

			return sampleCount;
		}


		private WaveFormat waveFormat;

		public void SetWaveFormat(int sampleRate, int channels)
		{
			this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			var waveBuffer = new WaveBuffer(buffer);
			int samplesRequired = count / 4;
			int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
			return samplesRead * 4;
		}

		public override WaveFormat WaveFormat
		{
			get { return waveFormat; }
		}


		public override long Length
		{
			get { return long.MaxValue; }
		}

		public override long Position
		{
			get
			{
				return 0;
			}
			set
			{
				long x = value;
			}
		}
	}
}
