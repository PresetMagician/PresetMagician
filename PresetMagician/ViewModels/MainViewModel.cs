using System.ComponentModel;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Helpers;
using PresetMagician.Services.Interfaces;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagician.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private LayoutDocumentPane _layoutDocumentPane;
        private IRuntimeConfigurationService _runtimeConfigurationService;

        public MainViewModel(IRuntimeConfigurationService runtimeConfigurationService)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            AvalonDockHelper.CreateDocument<VstPluginsViewModel>(activateDocument: true);
            AvalonDockHelper.CreateDocument<PresetExportListViewModel>();

            _layoutDocumentPane = ServiceLocator.Default.ResolveType<LayoutDocumentPane>();
            _layoutDocumentPane.PropertyChanged += LayoutDocumentPaneOnPropertyChanged;
            _runtimeConfigurationService = runtimeConfigurationService;
        }

        private void LayoutDocumentPaneOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedContent")
            {
                if (!(_layoutDocumentPane.SelectedContent is null) &&
                    !(_layoutDocumentPane.SelectedContent.Content is null))
                {
                    var type = _layoutDocumentPane.SelectedContent.Content.GetType();
                    _runtimeConfigurationService.ApplicationState.CurrentDocument = type;
                }
            }
        }
    }
}