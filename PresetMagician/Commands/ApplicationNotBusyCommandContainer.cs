using System.ComponentModel;
using Catel;
using Catel.MVVM;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician
{
    public abstract class ApplicationNotBusyCommandContainer: CommandContainerBase
    {
        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        
        protected ApplicationNotBusyCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(command, commandManager)
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
    }
}