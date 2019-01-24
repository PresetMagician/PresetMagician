using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CannedBytes.Midi.Message;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using ColorThiefDotNet;
using Orchestra.Services;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Views;
using SharedModels.NativeInstrumentsResources;
using Color = System.Drawing.Color;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Drachenkatze.PresetMagician.Utils;
using Brushes = System.Drawing.Brushes;
using ColorConverter = System.Drawing.ColorConverter;
using Pen = System.Drawing.Pen;
using Size = System.Drawing.Size;

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
                    ApplyMidiNote.FullNoteName = _vstService.SelectedExportPreset.PreviewNote.FullNoteName;
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
        public TaskCommand DoSomething { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnDoSomethingExecuteAsync()
        {
           


            
            
        }

        #endregion
    }
}