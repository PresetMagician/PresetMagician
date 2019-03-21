using System.ComponentModel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;
using PresetMagician.Helpers;
using PresetMagician.Models;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagician.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private LayoutDocumentPane _layoutDocumentPane;
        private ICommandManager _commandManager;
        private readonly GlobalFrontendService _globalFrontendService;

        public MainViewModel(ICommandManager commandManager, GlobalFrontendService globalFrontendService)
        {
            _layoutDocumentPane = ServiceLocator.Default.ResolveType<LayoutDocumentPane>();
            _layoutDocumentPane.PropertyChanged += LayoutDocumentPaneOnPropertyChanged;
            _commandManager = commandManager;
            _globalFrontendService = globalFrontendService;

            AvalonDockHelper.CreateDocument<VstPluginsViewModel>(activateDocument: true);
            AvalonDockHelper.CreateDocument<PresetExportListViewModel>();
        }

        private void LayoutDocumentPaneOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedContent")
            {
                if (_layoutDocumentPane.SelectedContent?.Content != null)
                {
                    var content = _layoutDocumentPane.SelectedContent as CustomLayoutDocument;
                    _globalFrontendService.ApplicationState.CurrentDocumentViewModel =
                        content.ViewModel;
                }
            }
        }
    }
}