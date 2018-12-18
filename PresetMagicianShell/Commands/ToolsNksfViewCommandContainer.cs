using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    public class ToolsNksfViewCommandContainer : CommandContainerBase
    {
        private const string ViewModelType = "NKSFViewModel";

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;

        public ToolsNksfViewCommandContainer(ICommandManager commandManager, IUIVisualizerService uiVisualizerService, IViewModelFactory viewModelFactory)
            : base(Commands.Tools.NksfView, commandManager)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);
            var j = TypeCache.GetTypes();

            var settingsViewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, ViewModelType)).FirstOrDefault();
            if (settingsViewModelType == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("Cannot find type '{0}'", ViewModelType);
            }

            var viewModel = _viewModelFactory.CreateViewModel(settingsViewModelType, null, null);

            await _uiVisualizerService.ShowDialogAsync(viewModel);
        }
    }
}