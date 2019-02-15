using System.ComponentModel;
using Catel;
using Catel.MVVM;
using Catel.MVVM.Converters;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician
{
    public abstract class ApplicationNotBusyCommandContainer: CommandContainerBase
    {
        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private bool _allowDuringEditing;
        
        protected ApplicationNotBusyCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, bool allowDuringEditing = false)
            : base(command, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            _runtimeConfigurationService = runtimeConfigurationService;
            _allowDuringEditing = allowDuringEditing;
            runtimeConfigurationService.ApplicationState.PropertyChanged += OnApplicationBusyChanged;
        }
        
        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && !_runtimeConfigurationService.ApplicationState.IsApplicationBusy &&
                   (_allowDuringEditing || !_runtimeConfigurationService.ApplicationState.IsApplicationEditing);
        }
        
        private void OnApplicationBusyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.IsApplicationBusy) ||
                ev.PropertyName == nameof(ApplicationState.IsApplicationEditing)) InvalidateCommand();
        }
    }
}