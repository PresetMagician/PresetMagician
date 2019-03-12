using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using CannedBytes.Midi.Message;
using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Runtime.Serialization;
using Catel.Services;
using Orchestra;
using Orchestra.Services;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Views;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using AssemblyHelper = Catel.Reflection.AssemblyHelper;
using Version = Redmine.Net.Api.Types.Version;

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

        public HelpLinks HelpLinks { get; } = new HelpLinks();

        public bool ShowDeveloperCommands
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

      

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

            Debug.WriteLine(_serviceLocator.ResolveType<DataPersisterService>().Plugins);
            

        }

   

        #endregion
    }
}