using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginEditorCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;

        public PluginToolsShowPluginEditorCommandContainer(ICommandManager commandManager, IVstService vstService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PluginTools.ShowPluginEditor, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.SelectedPlugins.Count == 1;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginInstance = _vstService.GetInteractivePluginInstance(_vstService.SelectedPlugin);

            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }

            pluginInstance.OpenEditor();
        }
    }
}