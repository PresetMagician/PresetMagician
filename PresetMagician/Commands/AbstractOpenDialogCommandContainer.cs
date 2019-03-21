using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    public abstract class AbstractOpenDialogCommandContainer : CommandContainerBase
    {
        protected static readonly ILog Log = LogManager.GetCurrentClassLogger();

        protected readonly IUIVisualizerService UiVisualizerService;
        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        protected readonly IViewModelFactory ViewModelFactory;
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly string _viewModel;
        private readonly bool _allowDuringApplicationBusy;

        protected AbstractOpenDialogCommandContainer(string commandName, string viewModel,
            bool allowDuringApplicationBusy,
            ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IRuntimeConfigurationService runtimeConfigurationService,
            IViewModelFactory viewModelFactory)
            : base(commandName, commandManager)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);
            Argument.IsNotNull(() => runtimeConfigurationService);

            UiVisualizerService = uiVisualizerService;
            ViewModelFactory = viewModelFactory;
            _viewModel = viewModel;
            _allowDuringApplicationBusy = allowDuringApplicationBusy;
            _runtimeConfigurationService = runtimeConfigurationService;
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _globalFrontendService.ApplicationState.PropertyChanged += OnApplicationBusyChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            if (!_allowDuringApplicationBusy)
            {
                return base.CanExecute(parameter) && !_globalFrontendService.ApplicationState.IsApplicationBusy;
            }

            return base.CanExecute(parameter);
        }

        private void OnApplicationBusyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.IsApplicationBusy)) InvalidateCommand();
        }

        protected virtual void OnBeforeShowDialog(IViewModel viewModel, object parameter)
        {
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            await base.ExecuteAsync(parameter);

            var viewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, _viewModel)).FirstOrDefault();
            if (viewModelType == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("Cannot find type '{0}'", _viewModel);
            }

            var viewModel = ViewModelFactory.CreateViewModel(viewModelType, null);

            OnBeforeShowDialog(viewModel, parameter);

            await UiVisualizerService.ShowDialogAsync(viewModel);
        }
    }
}