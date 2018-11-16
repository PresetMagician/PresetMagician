using System;
using System.Collections.Generic;

// Copied from the microDRUM project
// https://github.com/microDRUM
// I think it is created by massimo.bernava@gmail.com
namespace NAudio.Wave
{
	/// <summary>
	/// WaveStream that can mix together multiple 32 bit input streams
	/// (Normally used with stereo input channels)
	/// All channels must have the same number of inputs
	/// </summary>
	public class RecordableMixerStream32 : NAudio.Wave.WaveStream
	{
		private List<WaveStream> inputStreams;
		private WaveFormat waveFormat;
		private long length;
		private long position;
		private int bytesPerSample;
		private bool autoStop;

		// Declarations to support the streamToDisk recording methodology
		private bool streamToDisk;
		private string streamToDiskFileName;
		WaveFileWriter writer;

		/// <summary>
		/// Creates a new 32 bit WaveMixerStream
		/// </summary>
		public RecordableMixerStream32(int sampleRate, int channels)
		{
			this.autoStop = true;
			this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
			this.bytesPerSample = 4;
			this.inputStreams = new List<WaveStream>();
		}

		/// <summary>
		/// Add a new input to the mixer
		/// </summary>
		/// <param name="waveStream">The wave input to add</param>
		public void AddInputStream(WaveStream waveStream)
		{
			if (waveStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
				throw new ArgumentException("Must be IEEE floating point", "waveStream.WaveFormat");
			if (waveStream.WaveFormat.BitsPerSample != 32)
				throw new ArgumentException("Only 32 bit audio currently supported", "waveStream.WaveFormat");

			if (inputStreams.Count == 0)
			{
				int channels = waveStream.WaveFormat.Channels;
				// first one - set the format
				int sampleRate = waveStream.WaveFormat.SampleRate;
				this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
			}
			else
			{
				if (!waveStream.WaveFormat.Equals(waveFormat))
					throw new ArgumentException("All incoming channels must have the same format", "inputStreams.WaveFormat");
			}

			lock (this)
			{
				this.inputStreams.Add(waveStream);
				this.length = Math.Max(this.length, waveStream.Length);
				// get to the right point in this input file
				this.Position = Position;
			}
		}

		/// <summary>
		/// Remove a WaveStream from the mixer
		/// </summary>
		/// <param name="waveStream">waveStream to remove</param>
		public void RemoveInputStream(WaveStream waveStream)
		{
			lock (this)
			{
				if (this.inputStreams.Remove(waveStream))
				{
					// recalculate the length
					this.length = 0;
					foreach (WaveStream inputStream in inputStreams)
					{
						this.length = Math.Max(this.length, waveStream.Length);
					}
				}
			}
		}

		/// <summary>
		/// The number of inputs to this mixer
		/// </summary>
		public int InputCount
		{
			get { return this.inputStreams.Count; }
		}


		/// <summary>
		/// Creates a new 32 bit WaveMixerStream
		/// </summary>
		/// <param name="inputStreams">An Array of WaveStreams - must all have the same format.
		/// Use WaveChannel is designed for this purpose.</param>
		/// <param name="autoStop">Automatically stop when all inputs have been read</param>
		/// <exception cref="ArgumentException">Thrown if the input streams are not 32 bit floating point,
		/// or if they have different formats to each other</exception>
		public RecordableMixerStream32(IEnumerable<WaveStream> inputStreams, bool autoStop, int sampleRate, int channels)
			: this(sampleRate, channels)
		{
			this.autoStop = autoStop;
			
			foreach (WaveStream inputStream in inputStreams)
			{
				AddInputStream(inputStream);
			}
		}

		/// <summary>
		/// Automatically stop when all inputs have been read
		/// </summary>
		public bool AutoStop
		{
			get { return autoStop; }
			set { autoStop = value; }
		}


		/// <summary>
		/// Starts the Strem To Disk recording if a file name to save the stream to has been setup
		/// </summary>
		public void StartStreamingToDisk()
		{
			if (!String.IsNullOrEmpty(streamToDiskFileName))
			{
				streamToDisk = true;
			}
		}

		/// <summary>
		/// Pause's the stream to disk recording (No further blocks are written during the mixing)
		/// </summary>
		public void PauseStreamingToDisk()
		{
			streamToDisk = false;
		}

		/// <summary>
		/// Resume streaming to disk
		/// </summary>
		public void ResumeStreamingToDisk()
		{
			streamToDisk = true;
		}

		/// <summary>
		/// Stop the streaming to disk and clean up
		/// </summary>
		public void StopStreamingToDisk()
		{
			streamToDisk = false;
			if(writer!=null) writer.Close();
		}

		/// <summary>
		/// Setup the StreamMixToDisk file and initalise the WaveFileWriter
		/// </summary>
		/// <param name="FileName">FileName to save the mixed stream</param>
		public void StreamMixToDisk(string FileName)
		{
			streamToDiskFileName = FileName;
			writer = new WaveFileWriter(FileName, this.WaveFormat);
		}

		/// <summary>
		/// Using the final set of data passed through in the overriden read method to also be passed to the WaveFileWriter
		/// </summary>
		/// <param name="buffer">Data to be written</param>
		/// <param name="offset">The Offset, should be 0 as we are taking the mixed data to write and want it all</param>
		/// <param name="count">The total count of all the mixed data in the buffer</param>
		private void WriteMixStreamOut(byte[] buffer, int offset, int count)
		{
			// Write the data to the file
			writer.Write(buffer, offset, count);
		}

		/// <summary>
		/// Reads bytes from this wave stream
		/// </summary>
		/// <param name="buffer">buffer to read into</param>
		/// <param name="offset">offset into buffer</param>
		/// <param name="count">number of bytes required</param>
		/// <returns>Number of bytes read.</returns>
		/// <exception cref="ArgumentException">Thrown if an invalid number of bytes requested</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (autoStop)
			{
				if (position + count > length)
					count = (int)(length - position);

				// was a bug here, should be fixed now
				System.Diagnostics.Debug.Assert(count >= 0, "length and position mismatch");
			}


			if (count % bytesPerSample != 0)
				throw new ArgumentException("Must read an whole number of samples", "count");

			// blank the buffer
			Array.Clear(buffer, offset, count);
			int bytesRead = 0;

			// sum the channels in
			var readBuffer = new byte[count];
			foreach (WaveStream inputStream in inputStreams)
			{
				if (inputStream.HasData(count))
				{
					int readFromThisStream = inputStream.Read(readBuffer, 0, count);
					//System.Diagnostics.Debug.Assert(readFromThisStream == count, "A mixer input stream did not provide the requested amount of data");
					bytesRead = Math.Max(bytesRead, readFromThisStream);
					if (readFromThisStream > 0)
						Sum32BitAudio(buffer, offset, readBuffer, readFromThisStream);
				}
				else
				{
					bytesRead = Math.Max(bytesRead, count);
					inputStream.Position += count;
				}
			}
			position += count;
			// If streamToDisk has been enabled the mixed audio will be streamed directly to a wave file, so we need to send the data to the wave file writer
			if (streamToDisk)
			{
				WriteMixStreamOut(buffer, 0, count);
			}
			return count;
		}

		/// <summary>
		/// Actually performs the mixing
		/// </summary>
		/// 
		unsafe void Sum32BitAudio(byte[] destBuffer, int offset, byte[] sourceBuffer, int bytesRead)
		{
			fixed (byte* pDestBuffer = &destBuffer[offset],
			       pSourceBuffer = &sourceBuffer[0])
			{
				float* pfDestBuffer = (float*)pDestBuffer;
				float* pfReadBuffer = (float*)pSourceBuffer;
				int samplesRead = bytesRead / 4;
				//BlockEffects(samplesRead/2);
				for (int n = 0; n < samplesRead; n+=2)
				{
					pfDestBuffer[n] += pfReadBuffer[n];
					pfDestBuffer[n+1] += pfReadBuffer[n+1];
					//ApplyEffects(ref pfDestBuffer[n], ref pfDestBuffer[n + 1]);
				}
			}
		}

		/// <summary>
		/// <see cref="WaveStream.BlockAlign"/>
		/// </summary>
		public override int BlockAlign
		{
			get
			{
				return waveFormat.BlockAlign; // inputStreams[0].BlockAlign;
			}
		}

		/// <summary>
		/// Length of this Wave Stream (in bytes)
		/// <see cref="System.IO.Stream.Length"/>
		/// </summary>
		public override long Length
		{
			get
			{
				return length;
			}
		}

		/// <summary>
		/// Position within this Wave Stream (in bytes)
		/// <see cref="System.IO.Stream.Position"/>
		/// </summary>
		public override long Position
		{
			get
			{
				// all streams are at the same position
				return position;
			}
			set
			{
				lock (this)
				{
					value = Math.Min(value, Length);
					foreach (WaveStream inputStream in inputStreams)
					{
						inputStream.Position = Math.Min(value, inputStream.Length);
					}
					this.position = value;
				}
			}
		}

		/// <summary>
		/// <see cref="WaveStream.WaveFormat"/>
		/// </summary>
		public override WaveFormat WaveFormat
		{
			get
			{
				return waveFormat;
			}
		}

		/// <summary>
		/// Disposes this WaveStream
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (inputStreams != null)
				{
					foreach (WaveStream inputStream in inputStreams)
					{
						inputStream.Dispose();
					}
					inputStreams = null;
				}
			}
			else
			{
				System.Diagnostics.Debug.Assert(false, "WaveMixerStream32 was not disposed");
			}
			base.Dispose(disposing);
		}

		internal bool ContainsInputStream(WaveChannel32 waveChannel)
		{
			return inputStreams.Contains(waveChannel);
		}
	}
}
