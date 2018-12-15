using System;
using System.Collections.ObjectModel;
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
using PresetMagicianShell.Helpers;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Views;

namespace PresetMagicianShell.ViewModels
{
    
    public class MainViewModel : ViewModelBase
    {
        private readonly IVstService _vstService;

        public MainViewModel(IVstService vstService)
        {
            var document = AvalonDockHelper.CreateDocument<VstPluginsViewModel>();
            document.CanClose = false;
            var x = (VstPluginsView) document.Content;
            
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;
        }
    }
}