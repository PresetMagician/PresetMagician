using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using CannedBytes.Midi.Message;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using Orchestra.Services;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Views;

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
            Debug.WriteLine(_vstService.GetVstService().Exists(@"C:\foo.txt"));
        }

        #endregion
    }
}