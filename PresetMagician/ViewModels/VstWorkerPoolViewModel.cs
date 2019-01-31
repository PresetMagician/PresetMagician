using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Models;
using PresetMagician.ProcessIsolation;
using PresetMagician.ProcessIsolation.Processes;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.ViewModels
{
    public class VstWorkerPoolViewModel : ViewModelBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IUIVisualizerService _uiVisualizerService;

        public VstWorkerPoolViewModel(IApplicationService applicationService,
            IUIVisualizerService uiVisualizerService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => uiVisualizerService);
            _applicationService = applicationService;
            _uiVisualizerService = uiVisualizerService;

            ProcessPool = _applicationService.NewProcessPool;
            
            ShowVstHostProcessLog = new TaskCommand(OnShowVstHostProcessLogExecuteAsync, CanShowVstHostProcessLogExecute);
            KillVstHostProcess = new TaskCommand(OnKillVstHostProcessExecute, CanKillVstHostProcessExecute);
            StopPool = new TaskCommand(OnStopPoolExecute, CanStopPoolExecute);
            StartPool = new TaskCommand(OnStartPoolExecute, CanStartPoolExecute);
            
            ProcessPool.PropertyChanged += ProcessPoolOnPropertyChanged;

        }

        private void ProcessPoolOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewProcessPool.PoolRunning))
            {
                StopPool.RaiseCanExecuteChanged();
                StartPool.RaiseCanExecuteChanged();
            }
        }


        public NewProcessPool ProcessPool { get; }
        public VstHostProcess SelectedVstHostProcess { get; set; }
        
        public TaskCommand ShowVstHostProcessLog { get; private set; }

        private async Task OnShowVstHostProcessLogExecuteAsync()
        {
            await _uiVisualizerService.ShowDialogAsync<VstHostProcessLogViewModel>(SelectedVstHostProcess);
        }

        private bool CanShowVstHostProcessLogExecute()
        {
            if (SelectedVstHostProcess == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public TaskCommand KillVstHostProcess { get; }

        private async Task OnKillVstHostProcessExecute()
        {
            SelectedVstHostProcess.ForceStop("User Request");
        }
        
        private bool CanKillVstHostProcessExecute()
        {
            if (SelectedVstHostProcess == null)
            {
                return false;
            }

            return SelectedVstHostProcess.CurrentProcessState != HostProcess.ProcessState.EXITED;
        }
        
        public TaskCommand StopPool { get; }

        private async Task OnStopPoolExecute ()
        {
            ProcessPool.StopPool();
        }
        
        private bool CanStopPoolExecute()
        {
            return ProcessPool.PoolRunning;
        } 
        
        public TaskCommand StartPool { get; }

        private async Task OnStartPoolExecute ()
        {
            ProcessPool.StartPool();
        }
        
        private bool CanStartPoolExecute()
        {
            return !ProcessPool.PoolRunning;
        }
        
        
    }
}