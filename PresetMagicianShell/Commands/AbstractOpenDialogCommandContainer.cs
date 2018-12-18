using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell 
{
    public abstract class AbstractOpenDialogCommandContainer : CommandContainerBase
    {
        private string _viewModel;

        protected static readonly ILog Log = LogManager.GetCurrentClassLogger();

        protected readonly IUIVisualizerService UiVisualizerService;
        protected readonly IViewModelFactory ViewModelFactory;

        protected AbstractOpenDialogCommandContainer(string commandName, string viewModel, ICommandManager commandManager, IUIVisualizerService uiVisualizerService, IViewModelFactory viewModelFactory)
            : base(commandName, commandManager)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            UiVisualizerService = uiVisualizerService;
            ViewModelFactory = viewModelFactory;
            _viewModel = viewModel;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);

            var viewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, _viewModel)).FirstOrDefault();
            if (viewModelType == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("Cannot find type '{0}'", _viewModel);
            }

            var viewModel = ViewModelFactory.CreateViewModel(viewModelType, null);

            await UiVisualizerService.ShowDialogAsync(viewModel);
        }
    }
}
