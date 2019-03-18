using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.RemoteVstHost;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstWorkerPoolViewModel : ViewModelBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly GlobalService _globalService;

        public VstWorkerPoolViewModel(IApplicationService applicationService, GlobalService globalService,
            IUIVisualizerService uiVisualizerService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => uiVisualizerService);
            _applicationService = applicationService;
            _uiVisualizerService = uiVisualizerService;

            _globalService = globalService;
            ProcessPool = _globalService.RemoteVstHostProcessPool;
            
            ShowVstHostProcessLog = new TaskCommand(OnShowVstHostProcessLogExecuteAsync, CanShowVstHostProcessLogExecute);
            KillVstHostProcess = new TaskCommand(OnKillVstHostProcessExecute, CanKillVstHostProcessExecute);
            StopPool = new TaskCommand(OnStopPoolExecute, CanStopPoolExecute);
            StartPool = new TaskCommand(OnStartPoolExecute, CanStartPoolExecute);
            
            ProcessPool.PropertyChanged += ProcessPoolOnPropertyChanged;

        }

        private void ProcessPoolOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RemoteVstHostProcessPool.PoolRunning))
            {
                StopPool.RaiseCanExecuteChanged();
                StartPool.RaiseCanExecuteChanged();
            }
        }


        public IRemoteVstHostProcessPool ProcessPool { get; }
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

            return SelectedVstHostProcess.CurrentProcessState != ProcessState.EXITED;
        }
        
        public TaskCommand StopPool { get; }

        private async Task OnStopPoolExecute ()
        {
            _applicationService.ShutdownProcessPool();
        }
        
        private bool CanStopPoolExecute()
        {
            return ProcessPool.PoolRunning;
        } 
        
        public TaskCommand StartPool { get; }

        private async Task OnStartPoolExecute ()
        {
            _applicationService.StartProcessPool();
        }
        
        private bool CanStartPoolExecute()
        {
            return !ProcessPool.PoolRunning;
        }
        
        
    }
}