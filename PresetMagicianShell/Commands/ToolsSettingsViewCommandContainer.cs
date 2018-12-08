using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Catel.IoC;
using Xceed.Wpf.AvalonDock;
using PresetMagicianShell.Helpers;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell
{
    public class ToolsSettingsViewCommandContainer : CommandContainerBase
    {
        private const string ViewModelType = "SettingsViewModel";

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IServiceLocator _serviceLocator;

        public ToolsSettingsViewCommandContainer(ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory, IServiceLocator serviceLocator)
            : base(Commands.Tools.SettingsView, commandManager)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);
            Argument.IsNotNull(() => serviceLocator);

            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
            _serviceLocator = serviceLocator;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);

            AvalonDockHelper.CreateDocument<SettingsViewModel>();
        }
    }
}