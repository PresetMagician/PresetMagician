using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using CannedBytes.Midi.Message;
using Catel;
using Catel.Collections;
using Catel.Configuration;
using Catel.IoC;
using Catel.IO;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Runtime.Serialization;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Orchestra.Services;
using Orchestra.ViewModels;
using Portable.Licensing;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Views;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Win32Mapi;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagician.ViewModels
{
    public class RibbonViewModel : ViewModelBase
    {
        #region Fields

        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IServiceLocator _serviceLocator;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;

        public ApplicationState ApplicationState { get; private set; }
        public RuntimeConfiguration RuntimeConfiguration { get; private set; }

        #endregion Fields

        #region Constructors

        public RibbonViewModel(
            IUIVisualizerService uiVisualizerService,
            IServiceLocator serviceLocator,
            IRuntimeConfigurationService runtimeConfigurationService,
            IVstService vstService
           )
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            
            _uiVisualizerService = uiVisualizerService;
            _serviceLocator = serviceLocator;
            _vstService = vstService;
            _runtimeConfigurationService = runtimeConfigurationService;

            _vstService.SelectedPresets.CollectionChanged += OnSelectedPresetsListChanged;

            ApplicationState = runtimeConfigurationService.ApplicationState;
            RuntimeConfiguration = runtimeConfigurationService.RuntimeConfiguration;

            Title = AssemblyHelper.GetEntryAssembly().Title();
            ShowAboutDialog = new TaskCommand(OnShowAboutDialogExecuteAsync);
            ShowThemeTest = new TaskCommand(OnShowThemeTestExecuteAsync);
            ResetDock = new TaskCommand(OnResetDockExecuteAsync);
            DoSomething = new TaskCommand(OnDoSomethingExecuteAsync);
        }
        #endregion
        
        #region Properties
        public bool HasPresetSelection { get; set; }
        public MidiNoteName ApplyMidiNote { get; set; } = new MidiNoteName();
        #endregion

        private void OnSelectedPresetsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (_vstService.SelectedPresets.Count > 0)
            {
                HasPresetSelection = true;

                if (_vstService.SelectedPresets.Count == 1)
                {
                    Debug.WriteLine(_vstService.SelectedExportPreset.PreviewNote.FullNoteName);
                    ApplyMidiNote.FullNoteName = _vstService.SelectedExportPreset.PreviewNote.FullNoteName;
                    Debug.WriteLine(ApplyMidiNote.FullNoteName);
                }
                else
                {
                    ApplyMidiNote.FullNoteName = "";
                }
            }
            else
            {
                HasPresetSelection = false;
            }
        }
        
        #region Commands
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
            var mainView = ServiceLocator.Default.ResolveType<LayoutAnchorable>("PluginSettings");
            
            //mainView.ToggleAutoHide();
        }

        
       
        #endregion 
    }
}