using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel.Collections;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IRemoteVstHostProcessPool
    {
        event EventHandler<PoolFailedEventArgs> PoolFailed;
        FastObservableCollection<IVstHostProcess> RunningProcesses { get; }
        FastObservableCollection<IVstHostProcess> OldProcesses { get; }
        bool PoolRunning { get; }
        void StartPool();
        void SetMaxProcesses(int maxProcesses);
        IRemoteVstService GetVstService();
        void SetStartTimeout(int maxStartTimeoutSeconds);
        IVstHostProcess GetFreeHostProcess();
        void StopPool();
        IRemotePluginInstance GetRemotePluginInstance(Plugin plugin, bool backgroundProcessing = true);
        event PropertyChangedEventHandler PropertyChanged;
        int NumRunningProcesses { get; set; }
        int NumTotalProcesses { get; set; }
        IRemotePluginInstance GetRemoteInteractivePluginInstance(Plugin plugin, bool backgroundProcessing = true);
        void SetMinProcesses(int minProcesses);
    }
}