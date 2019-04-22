using System;
using System.Drawing;
using System.Text;
using System.Windows;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Deprecated;
using Jacobi.Vst.Core.Host;
using PresetMagician.Utils.Logger;
using PresetMagician.VstHost.VST;

namespace Drachenkatze.PresetMagician.VSTHost
{
    public class NewHostCommandStub : IVstHostCommandStub, IVstHostCommandsDeprecated20, IDisposable
    {
        public string Directory;
        public string PluginDll;
        private MiniLogger _logger;
        private bool _debug;
        private VstProcessLevels _currentProcessLevel;

        public NewHostCommandStub(MiniLogger logger)
        {
            _debug = true;

            _logger = logger;
            _currentProcessLevel = VstProcessLevels.User;
        }

        public void Dispose()
        {
        }

        private void Debug(string message)
        {
            if (_debug)
            {
                _logger.Debug($"{PluginDll} called: {message}");
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
            Debug($"GetBlockSize => {VstHost.BlockSize}");
            return VstHost.BlockSize;
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
            if (PluginDll.ToLower() == "wavestation.dll")
            {
                return null;
            }

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
            var plugin = PluginContext.Find<RemoteVstPlugin>("Plugin");
            plugin?.ResizeEditor(width, height);
            return true;
        }

        public bool UpdateDisplay()
        {
            Debug($"UpateDisplay");
            var plugin = PluginContext.Find<RemoteVstPlugin>("Plugin");
            plugin?.RedrawEditor();

            return true;
        }

        #endregion IVstHostCommands20 Members

        #region IVstHostCommands10 Members

        public int GetCurrentPluginID()
        {
            Debug($"GetCurrentPluginID");
            return 0; // This is the shell plugin ID, which should be 0
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

        #region IVstHostCommandsDeprecated20

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
            var plugin = PluginContext.Find<RemoteVstPlugin>("Plugin");

#if DEBUG
            /*VstPluginCommandAdapter.Create(plugin.PluginContext.PluginCommandStub)
            if (plugin != null)
            {
                
            } is IVstPluginCommandsDeprecated10)
            {
                    
            }*/

            var sb = new StringBuilder();

            foreach (var intf in plugin.PluginContext.PluginCommandStub.GetType().GetInterfaces())
            {
                sb.AppendLine(intf.FullName);
            }

            MessageBox.Show("Plugin wants idle" + sb.ToString());

#endif
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

        #endregion
    }
}