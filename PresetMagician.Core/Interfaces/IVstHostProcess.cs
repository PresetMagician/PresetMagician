using System;
using System.ComponentModel;
using PresetMagician.Core.Models;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Utils.Logger;

namespace PresetMagician.Core.Interfaces
{
    public enum ProcessState
    {
        STARTING,
        RUNNING,
        EXITED
    }

    public interface IVstHostProcess
    {
        ProcessOperation CurrentOperation { get; }
        int Pid { get; }
        DateTime StartDateTime { get; }
        string StartupTimeFormatted { get; }
        DateTime StopDateTime { get; }
        long MemoryUsage { get; }
        ProcessState CurrentProcessState { get; }
        string Logs { get; }
        MiniMemoryLogger Logger { get; }
        bool IsBusy { get; }
        TimeSpan StartupTime { get; }
        TimeSpan Uptime { get; }
        string StopReason { get; }
        bool StartupSuccessful { get; }
        void WaitUntilStarted();
        void Lock(Plugin plugin);
        bool IsLockedToPlugin();
        Plugin GetLockedPlugin();
        void Unlock();
        void Start();
        void ResetPingTimer();
        bool StartOperation(string operation);
        void StopOperation(string operation, string result = "OK");
        bool IsRemoteVstServiceAvailable();
        IRemoteVstService GetVstService();
        event EventHandler ProcessStateUpdated;
        void ForceStop(string reason);
        event PropertyChangedEventHandler PropertyChanged;
    }
}