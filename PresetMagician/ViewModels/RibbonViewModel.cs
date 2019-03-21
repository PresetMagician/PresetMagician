using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Orchestra.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Views;

namespace PresetMagician.ViewModels
{
    public class RibbonViewModel : ViewModelBase
    {
        #region Fields

        private readonly GlobalService _globalService;
        public ApplicationState ApplicationState { get; }
        public RuntimeConfiguration RuntimeConfiguration { get; }

        #endregion Fields

        #region Constructors

        public RibbonViewModel(
            GlobalFrontendService globalFrontendService,
            GlobalService globalService
        )
        {
            ApplicationState = globalFrontendService.ApplicationState;
            RuntimeConfiguration = globalService.RuntimeConfiguration;

            Title = AssemblyHelper.GetEntryAssembly().Title();
            ShowAboutDialog = new TaskCommand(OnShowAboutDialogExecuteAsync);
            ShowThemeTest = new TaskCommand(OnShowThemeTestExecuteAsync);
            DoSomething = new TaskCommand(OnDoSomethingExecuteAsync);
        }

        #endregion

        #region Properties

        public bool HasPresetSelection { get; set; }

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
            //Debug.WriteLine(_serviceLocator.ResolveType<DataPersisterService>().Plugins);
        }

        #endregion
    }
}