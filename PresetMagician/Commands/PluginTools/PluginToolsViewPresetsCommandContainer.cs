using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using MethodTimer;
using PresetMagician.Core.Models;
using PresetMagician.Helpers;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewPresetsCommandContainer : AbstractOpenDialogCommandContainer
    {
        public PluginToolsViewPresetsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator
        )
            : base(Commands.PluginTools.ViewPresets, nameof(VstPluginPresetsViewModel), true, commandManager, serviceLocator)
        {
            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }


        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count > 0 &&
                   _globalFrontendService.SelectedPlugin != null;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
          
            InvalidateCommand();
        }


        protected override object GetModel()
        {
            return _globalFrontendService.SelectedPlugin;
        }
    }
}