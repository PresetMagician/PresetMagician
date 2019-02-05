using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician
{
    public abstract class ThreadedApplicationNotBusyCommandContainer: ApplicationNotBusyCommandContainer
    {
        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        
        protected ThreadedApplicationNotBusyCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(command, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            _runtimeConfigurationService = runtimeConfigurationService;
            runtimeConfigurationService.ApplicationState.PropertyChanged += OnApplicationBusyChanged;
        }
        
        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && !_runtimeConfigurationService.ApplicationState.IsApplicationBusy;
        }
        
        private void OnApplicationBusyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.IsApplicationBusy)) InvalidateCommand();
        }

        protected async virtual Task ExecuteThreaded(object parameter)
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            try
            {
                await TaskHelper.Run(async () => await ExecuteThreaded(parameter));
            }
            catch (Exception e)
            {
                LogTo.Error($"Error executing command {CommandName} - Got exception {e.GetType().FullName} with message {e.Message}");
                LogTo.Debug(e.StackTrace);
            }
        }
    }
}