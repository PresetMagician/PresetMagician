using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Configuration;
using Catel.IoC;
using Catel.IO;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Runtime.Serialization;
using Catel.Services;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Orchestra.Services;
using Orchestra.ViewModels;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Views;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace PresetMagicianShell.ViewModels
{
    public class RibbonViewModel : ViewModelBase
    {
        #region Fields

        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IServiceLocator _serviceLocator;
        #endregion Fields

        #region Constructors

        public RibbonViewModel(
            IUIVisualizerService uiVisualizerService,
            IServiceLocator serviceLocator
           )
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => serviceLocator);
            
            _uiVisualizerService = uiVisualizerService;
            _serviceLocator = serviceLocator;
            
            Title = AssemblyHelper.GetEntryAssembly().Title();
            ShowAboutDialog = new TaskCommand(OnShowAboutDialogExecuteAsync);
            ShowThemeTest = new TaskCommand(OnShowThemeTestExecuteAsync);
            ResetDock = new TaskCommand(OnResetDockExecuteAsync);
            DoSomething = new TaskCommand(OnDoSomethingExecuteAsync);
        }

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand ShowAboutDialog { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnShowAboutDialogExecuteAsync()
        {
            var aboutService = ServiceLocator.Default.ResolveType<IAboutService>();
            await aboutService.ShowAboutAsync();
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand ShowThemeTest { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnShowThemeTestExecuteAsync()
        {
            var aboutService = new ThemeControlsWindow();
            aboutService.Show();
        }

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand ResetDock { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnResetDockExecuteAsync()
        {
            var serviceLocator = ServiceLocator.Default;
            var x = serviceLocator.ResolveType<IRuntimeConfigurationService>();
            x.ResetLayout();
        }

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand DoSomething { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnDoSomethingExecuteAsync()
        {
            var pluginPath = @"C:\Users\Drachenkatze\Documents\TestCrash\PolyGAS64.dll";

            var vsthost = new VstHost();
            var plugin = new Plugin();
            plugin.DllPath = pluginPath;

            MessageBox.Show("yolo1", "yolo1");
            vsthost.LoadVST(plugin);
            
            MessageBox.Show("yolo", "yolo");
            
        }

        
       
        #endregion Constructors
    }
}