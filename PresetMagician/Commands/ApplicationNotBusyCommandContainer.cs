using System.ComponentModel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician
{
    public abstract class ApplicationNotBusyCommandContainer : CommandContainerBase
    {
        protected readonly GlobalFrontendService _globalFrontendService;
        protected readonly IRuntimeConfigurationService RuntimeConfigurationService;
        protected readonly IServiceLocator ServiceLocator;
        private bool _allowDuringEditing;

        protected ApplicationNotBusyCommandContainer(string command, ICommandManager commandManager,
            IServiceLocator serviceLocator, bool allowDuringEditing = false)
            : base(command, commandManager)
        {
            ServiceLocator = serviceLocator;
            RuntimeConfigurationService = ServiceLocator.ResolveType<IRuntimeConfigurationService>();
            _globalFrontendService = ServiceLocator.ResolveType<GlobalFrontendService>();
            _allowDuringEditing = allowDuringEditing;
            _globalFrontendService.ApplicationState.PropertyChanged += OnApplicationBusyChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && !_globalFrontendService.ApplicationState.IsApplicationBusy &&
                   (_allowDuringEditing || !_globalFrontendService.ApplicationState.IsApplicationEditing);
        }

        private void OnApplicationBusyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.IsApplicationBusy) ||
                ev.PropertyName == nameof(ApplicationState.IsApplicationEditing)) InvalidateCommand();
        }
    }
}