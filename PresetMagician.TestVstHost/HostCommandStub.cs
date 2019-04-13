using System;
using System.Drawing;
using System.IO;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Deprecated;
using Jacobi.Vst.Core.Host;

namespace PresetMagician.TestVstHost
{
    public class HostCommandStub : IVstHostCommandStub, IVstHostCommandsDeprecated20, IDisposable
    {
        public string Directory;
        public string PluginDll;
        private bool _debug;
        private VstProcessLevels _currentProcessLevel;

        public HostCommandStub()
        {
            _debug = true;

            _currentProcessLevel = VstProcessLevels.User;
        }

        public void Dispose()
        {
        }

        private void Debug(string message)
        {
            if (_debug)
            {
                File.AppendAllText(@"C:\Users\Drachenkatze\Desktop\log.txt", message+Environment.NewLine);
                Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        #region IVstHostCommandStub Members

        public IVstPluginContext PluginContext { get; set; }

        #endregion IVstHostCommandStub Members

        #region IVstHostCommands20 Members

        public bool BeginEdit(int index)
        {
            Debug($"BeginEdit {index}");
            return false;
        }


        public bool CloseFileSelector(VstFileSelect fileSelect)
        {
            Debug($"CloseFileSelector {fileSelect.Title} {fileSelect.InitialPath}");
            return false;
        }

        public bool EndEdit(int index)
        {
            Debug($"EndEdit {index}");
            return false;
        }

        public VstAutomationStates GetAutomationState()
        {
            Debug($"GetAutomationState");
            throw new NotImplementedException();
        }

        public int GetBlockSize()
        {
            Debug($"GetBlockSize => {1024}");
            return 1024;
        }

        public string GetDirectory()
        {
            Debug($"GetDirectory");
            return Directory;
        }

        public int GetInputLatency()
        {
            Debug($"GetInputLatency");
            return 0;
        }

        public VstHostLanguage GetLanguage()
        {
            Debug($"GetLanguage");
            return VstHostLanguage.English;
        }

        public int GetOutputLatency()
        {
            Debug($"GetOutputLatency");
            return 0;
        }

        public VstProcessLevels GetProcessLevel()
        {
            Debug($"GetProcessLevel => {_currentProcessLevel}");
            return _currentProcessLevel;
        }

        public void SetProcessLevel(VstProcessLevels processLevel)
        {
            _currentProcessLevel = processLevel;
        }

        public string GetProductString()
        {
            Debug($"GetProductString");
            

            return "PresetMagician";
        }

        public float GetSampleRate()
        {
            Debug($"GetSampleRate => 44100f");
            return 44100f;
        }

        private VstTimeInfo vstTimeInfo = new VstTimeInfo();

        public VstTimeInfo GetTimeInfo(VstTimeInfoFlags filterFlags)
        {
            Debug($"GetTimeInfo {filterFlags}");

            vstTimeInfo.SamplePosition = 0.0;
            vstTimeInfo.SampleRate = 44100;
            vstTimeInfo.NanoSeconds = 0.0;
            vstTimeInfo.PpqPosition = 2.0;
            vstTimeInfo.Tempo = 120.0;
            vstTimeInfo.BarStartPosition = 0.0;
            vstTimeInfo.CycleStartPosition = 0.0;
            vstTimeInfo.CycleEndPosition = 0.0;
            vstTimeInfo.TimeSignatureNumerator = 4;
            vstTimeInfo.TimeSignatureDenominator = 4;
            vstTimeInfo.SmpteOffset = 0;
            vstTimeInfo.SmpteFrameRate = new VstSmpteFrameRate();
            vstTimeInfo.SamplesToNearestClock = 0;
            vstTimeInfo.Flags = filterFlags;

            return vstTimeInfo;
        }

        public string GetVendorString()
        {
            Debug($"GetVendorString");
            return "Drachenkatze";
        }

        public int GetVendorVersion()
        {
            Debug($"GetVendorVersion");
            return 1000;
        }

        public bool IoChanged()
        {
            Debug($"IoChanged");
            return false;
        }

        /// <inheritdoc />
        public bool OpenFileSelector(VstFileSelect fileSelect)
        {
            Debug($"OpenFileSelector");
            return false;
        }

        public bool ProcessEvents(VstEvent[] events)
        {
            Debug($"ProcessEvents");
            return false;
        }

        /// <inheritdoc />
        public bool SizeWindow(int width, int height)
        {
            Debug($"SizeWindow {width}x{height}");
           
            return true;
        }

        public bool UpdateDisplay()
        {
            Debug($"UpateDisplay");
        
            return true;
        }

        #endregion IVstHostCommands20 Members

        #region IVstHostCommands10 Members

        public int GetCurrentPluginID()
        {
            Debug($"GetCurrentPluginID");
            return PluginContext.PluginInfo.PluginID;
        }

        public int GetVersion()
        {
            Debug($"GetVersion => 2400");
            return 2400;
        }

        public void ProcessIdle()
        {
            Debug($"ProcessIdle");
        }

        public void SetParameterAutomated(int index, float value)
        {
            Debug($"SetParameterAutomated {index} {value}");
        }

        public VstCanDoResult CanDo(string cando)
        {
            Debug($"CanDo {cando}");
            switch (cando)
            {
                case "NIMKPIVendorSpecificCallbacks":
                case "sendVstEvents":
                case "sendVstTimeInfo":
                case "receiveVstEvents":
                case "reportConnectionChanges":
                case "asyncProcessing":
                case "offline":
                case "supplyIdle":
                case "supportShell":
                case "openFileSelector":
                case "editFile":
                case "closeFileSelector":
                case "receiveVstTimeInfo":
                case "receiveVstMidiEvent":
                case "acceptIOChanges":
                case "notifySessionRestore":
                    return VstCanDoResult.No;
                case "sendVstMidiEvent":
                case "sizeWindow":
                    return VstCanDoResult.Yes;
                default:
#if DEBUG
                    throw new NotImplementedException("in CanDo with " + cando);
#endif
                    return VstCanDoResult.Unknown;
            }
        }

        #endregion IVstHostCommands10 Members

        #region IVstHostCommands10Deprecated

        public bool PinConnected(int connectionIndex, bool output)
        {
            Debug($"PinConnected {connectionIndex} {output}");
            if (!output && PluginContext.PluginInfo != null && PluginContext.PluginInfo.AudioInputCount < 2)
            {
                return true;
            }

            if (connectionIndex < 2)
            {
                return false;
            }

            return true;
        }

        #endregion


        public bool WantMidi()
        {
            Debug($"WantMidi");
            return false;
        }

        public bool SetTime(VstTimeInfo timeInfo, VstTimeInfoFlags filterFlags)
        {
            Debug($"SetTime");
            throw new NotImplementedException();
        }

        public int GetTempoAt(int sampleIndex)
        {
            Debug($"GetTempoAt");
            throw new NotImplementedException();
        }

        public int GetAutomatableParameterCount()
        {
            Debug($"GetAutomatableParameterCount");
            throw new NotImplementedException();
        }

        public int GetParameterQuantization(int parameterIndex)
        {
            Debug($"GetParameterQuantization");
            throw new NotImplementedException();
        }

        public bool NeedIdle()
        {
            
            Debug($"NeedIdle");
            // todo call idle on plugin there plugin?.PluginContext.PluginCommandStub.idle;   
            return true;
        }

        public IntPtr GetPreviousPlugin(int pinIndex)
        {
            Debug($"GetPreviousPlugin");
            throw new NotImplementedException();
        }

        public IntPtr GetNextPlugin(int pinIndex)
        {
            Debug($"GetNextPlugin");
            throw new NotImplementedException();
        }

        public int WillReplaceOrAccumulate()
        {
            Debug($"WillReplaceOrAccumulate");
            throw new NotImplementedException();
        }

        public bool SetOutputSampleRate(float sampleRate)
        {
            Debug($"SetOutputSampleRate {sampleRate}");
            throw new NotImplementedException();
        }

        public VstSpeakerArrangement GetOutputSpeakerArrangement()
        {
            Debug($"GetOutputSpeakerArrangement");
            throw new NotImplementedException();
        }

        public bool SetIcon(Icon icon)
        {
            Debug($"SetIcon");
            throw new NotImplementedException();
        }

        public IntPtr OpenWindow()
        {
            Debug($"OpenWindow");
            throw new NotImplementedException();
        }

        public bool CloseWindow(IntPtr wnd)
        {
            Debug($"CloseWindow");
            throw new NotImplementedException();
        }

        public bool EditFile(string xml)
        {
            Debug($"EditFile");
            throw new NotImplementedException();
        }

        public string GetChunkFile()
        {
            Debug($"GetChunkFile");
            return "";
        }

        public VstSpeakerArrangement GetInputSpeakerArrangement()
        {
            Debug($"GetInputSpeakerArrangement");
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Event arguments used when one of the mehtods is called.
    /// </summary>
    class PluginCalledEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance with a <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        public PluginCalledEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; private set; }
    }
}