using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Reflection;
using PresetMagician.Helpers;
using PresetMagician.Services.Interfaces;
using PresetMagician.Views;
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
            AvalonDockHelper.CreateDocument<VstPluginsViewModel>(activateDocument:true);
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