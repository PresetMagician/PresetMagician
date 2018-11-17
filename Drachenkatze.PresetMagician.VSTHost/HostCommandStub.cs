using System;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Core;
using System.Diagnostics;

namespace Drachenkatze.PresetMagician.VSTHost
{
    public class HostCommandStub : IVstHostCommandStub
    {
        public string Directory;

        public event EventHandler<PluginCalledEventArgs> PluginCalled;

        private void RaisePluginCalled(string message)
        {
            EventHandler<PluginCalledEventArgs> handler = PluginCalled;

            if (handler != null)
            {
                handler(this, new PluginCalledEventArgs(message));
            }
        }

        #region IVstHostCommandStub Members

        public IVstPluginContext PluginContext { get; set; }

        #endregion IVstHostCommandStub Members

        #region IVstHostCommands20 Members

        public bool BeginEdit(int index)
        {
            Debug.WriteLine("BeginEdit");
            return false;
        }

        public VstCanDoResult CanDo(VstHostCanDo cando)
        {
            Debug.WriteLine("CanDo");
            return VstCanDoResult.Unknown;
        }

        public bool CloseFileSelector(VstFileSelect fileSelect)
        {
            throw new NotImplementedException();
        }

        public bool EndEdit(int index)
        {
            Debug.WriteLine("EndEdit");
            return false;
        }

        public VstAutomationStates GetAutomationState()
        {
            throw new NotImplementedException();
        }

        public int GetBlockSize()
        {
            Debug.WriteLine("FOO");
            return 512;
        }

        public string GetDirectory()
        {
            return Directory;
        }

        public int GetInputLatency()
        {
            return 0;
        }

        public VstHostLanguage GetLanguage()
        {
            throw new NotImplementedException();
        }

        public int GetOutputLatency()
        {
            return 0;
        }

        public VstProcessLevels GetProcessLevel()
        {
            Debug.WriteLine("ProcessLevel");
            return Jacobi.Vst.Core.VstProcessLevels.Unknown;
        }

        public string GetProductString()
        {
            return "ProductString";
        }

        public float GetSampleRate()
        {
            return 44100f;
        }

        private Jacobi.Vst.Core.VstTimeInfo vstTimeInfo = new Jacobi.Vst.Core.VstTimeInfo();

        public VstTimeInfo GetTimeInfo(VstTimeInfoFlags filterFlags)
        {
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
            return "VendorString";
        }

        public int GetVendorVersion()
        {
            return 2400;
        }

        public bool IoChanged()
        {
            throw new NotImplementedException();
        }

        public bool OpenFileSelector(VstFileSelect fileSelect)
        {
            throw new NotImplementedException();
        }

        public bool ProcessEvents(VstEvent[] events)
        {
            Debug.WriteLine("ProcessEvents");
            return false;
        }

        public bool SizeWindow(int width, int height)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDisplay()
        {
            Debug.WriteLine("UpdateDisplay");
            return true;
        }

        #endregion IVstHostCommands20 Members

        #region IVstHostCommands10 Members

        public int GetCurrentPluginID()
        {
            return PluginContext.PluginInfo.PluginID;
        }

        public int GetVersion()
        {
            return 2400;
        }

        public void ProcessIdle()
        {
            return;
        }

        public void SetParameterAutomated(int index, float value)
        {
            Debug.WriteLine("SetParameterAutomated");
        }

        public VstCanDoResult CanDo(string cando)
        {
            Debug.WriteLine("CanDo10");
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