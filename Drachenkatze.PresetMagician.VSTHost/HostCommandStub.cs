using System;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Core;
using System.Diagnostics;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VSTHost
{
    public class HostCommandStub : IVstHostCommandStub
    {
        public string Directory;

        public event EventHandler<PluginCalledEventArgs> PluginCalled;

        private void RaisePluginCalled(string message)
        {
            //LogTo.Debug(message);
            /*EventHandler<PluginCalledEventArgs> handler = PluginCalled;

            if (handler != null)
            {
                handler(this, new PluginCalledEventArgs(message));
            }*/
        }

        #region IVstHostCommandStub Members

        public IVstPluginContext PluginContext { get; set; }

        #endregion IVstHostCommandStub Members

        #region IVstHostCommands20 Members

        public bool BeginEdit(int index)
        {
            RaisePluginCalled("BeginEdit");
            return false;
        }

        public VstCanDoResult CanDo(VstHostCanDo cando)
        {
            RaisePluginCalled("CanDo");
            return VstCanDoResult.Unknown;
        }

        public bool CloseFileSelector(VstFileSelect fileSelect)
        {
            RaisePluginCalled("CloseFileSelector");
            throw new NotImplementedException();
        }

        public bool EndEdit(int index)
        {
            RaisePluginCalled("EndEdit");
            return false;
        }

        public VstAutomationStates GetAutomationState()
        {
            RaisePluginCalled("GetAutomationStates");
            throw new NotImplementedException();
        }

        public int GetBlockSize()
        {
            RaisePluginCalled("GetBlockSize");
            return VstHost.BlockSize;
        }

        public string GetDirectory()
        {
            RaisePluginCalled("GetDirectory");
            return Directory;
        }

        public int GetInputLatency()
        {
            RaisePluginCalled("GetInputLatency");
            return 0;
        }

        public VstHostLanguage GetLanguage()
        {
            RaisePluginCalled("GetLanguage");
            throw new NotImplementedException();
        }

        public int GetOutputLatency()
        {
            RaisePluginCalled("GetOutputLatency");
            return 0;
        }

        public VstProcessLevels GetProcessLevel()
        {
            //RaisePluginCalled("GetProcessLevel");
            return Jacobi.Vst.Core.VstProcessLevels.Realtime;
        }

        public string GetProductString()
        {
            //RaisePluginCalled("GetProductString");
            return "PresetMagician";
        }

        public float GetSampleRate()
        {
            RaisePluginCalled("GetSampleRate");
            return 44100f;
        }

        private Jacobi.Vst.Core.VstTimeInfo vstTimeInfo = new Jacobi.Vst.Core.VstTimeInfo();

        public VstTimeInfo GetTimeInfo(VstTimeInfoFlags filterFlags)
        {
            //RaisePluginCalled("GetTimeInfo");
            
            vstTimeInfo.SamplePosition = 0.0;
            vstTimeInfo.SampleRate = 44100;
            vstTimeInfo.NanoSeconds = 0.0;
            vstTimeInfo.PpqPosition = 0.0;
            vstTimeInfo.Tempo = 120.0;
            vstTimeInfo.BarStartPosition = 0.0;
            vstTimeInfo.CycleStartPosition = 0.0;
            vstTimeInfo.CycleEndPosition = 0.0;
            vstTimeInfo.TimeSignatureNumerator = 4;
            vstTimeInfo.TimeSignatureDenominator = 4;
            vstTimeInfo.SmpteOffset = 0;
            vstTimeInfo.SmpteFrameRate = new Jacobi.Vst.Core.VstSmpteFrameRate();
            vstTimeInfo.SamplesToNearestClock = 0;
            vstTimeInfo.Flags = 0;

            return vstTimeInfo;
        }

        public string GetVendorString()
        {
            RaisePluginCalled("GetVendorString");
            return "Drachenkatze";
        }

        public int GetVendorVersion()
        {
            RaisePluginCalled("GetVendorVersion");
            return 2400;
        }

        public bool IoChanged()
        {
            RaisePluginCalled("IoChanged()");
            return false;
        }

        /// <inheritdoc />
        public bool OpenFileSelector(Jacobi.Vst.Core.VstFileSelect fileSelect)
        {
            RaisePluginCalled("OpenFileSelector(" + fileSelect.Command + ")");
            return false;
        }

        public bool ProcessEvents(VstEvent[] events)
        {
            RaisePluginCalled("ProcessEvents");
            return false;
        }

        /// <inheritdoc />
        public bool SizeWindow(int width, int height)
        {
            RaisePluginCalled("SizeWindow(" + width + ", " + height + ")");
            return false;
        }

        public bool UpdateDisplay()
        {
            RaisePluginCalled("UpdateDisplay");
            return true;
        }

        #endregion IVstHostCommands20 Members

        #region IVstHostCommands10 Members

        public int GetCurrentPluginID()
        {
            RaisePluginCalled("GetCurrentPluginID");
            return PluginContext.PluginInfo.PluginID;
        }

        public int GetVersion()
        {
            RaisePluginCalled("GetVersion");
            return 2400;
        }

        public void ProcessIdle()
        {
            RaisePluginCalled("ProcessIdle");
        }

        public void SetParameterAutomated(int index, float value)
        {
            //RaisePluginCalled("SetParameterAutomated");
        }

        public VstCanDoResult CanDo(string cando)
        {
            RaisePluginCalled("CanDo2 "+cando);
            return VstCanDoResult.Unknown;
        }

        #endregion IVstHostCommands10 Members
    }

    /// <summary>
    /// Event arguments used when one of the mehtods is called.
    /// </summary>
    public class PluginCalledEventArgs : EventArgs
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