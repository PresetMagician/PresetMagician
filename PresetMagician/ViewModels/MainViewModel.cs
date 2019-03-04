using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Services;
using Catel.Threading;
using PresetMagician.Helpers;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagician.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private LayoutDocumentPane _layoutDocumentPane;
        private IRuntimeConfigurationService _runtimeConfigurationService;
        private ICommandManager _commandManager;

        public MainViewModel(IRuntimeConfigurationService runtimeConfigurationService, ICommandManager commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);


            _layoutDocumentPane = ServiceLocator.Default.ResolveType<LayoutDocumentPane>();
            _layoutDocumentPane.PropertyChanged += LayoutDocumentPaneOnPropertyChanged;
            _runtimeConfigurationService = runtimeConfigurationService;
            _commandManager = commandManager;

            AvalonDockHelper.CreateDocument<VstPluginsViewModel>(activateDocument: true);
            AvalonDockHelper.CreateDocument<PresetExportListViewModel>();
        }

        private void LayoutDocumentPaneOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedContent")
            {
                if (_layoutDocumentPane.SelectedContent?.Content != null)
                {
                    var type = _layoutDocumentPane.SelectedContent.Content.GetType();
                    _runtimeConfigurationService.ApplicationState.CurrentDocumentType = type;

                    var content = _layoutDocumentPane.SelectedContent as CustomLayoutDocument;
                    _runtimeConfigurationService.ApplicationState.CurrentDocumentViewModel =
                        content.ViewModel as IViewModel;
                }
            }
        }
    }
}