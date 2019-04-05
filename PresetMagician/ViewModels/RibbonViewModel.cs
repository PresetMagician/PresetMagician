using System;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using Orchestra.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
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
            var ms = ServiceLocator.Default.ResolveType<IAdvancedMessageService>();
            
            var result = await ms.ShowCustomRememberMyChoiceDialogAsync(
                "There are unsupported plugins which are not reported." +
                Environment.NewLine +
                "Would you like to report them, so we can implement support for them?",
                "Report Unsupported Plugins", HelpLinks.COMMANDS_ANALYZE, MessageButton.YesNo, MessageImage.Question,
                "Don't ask again for the currently unreported plugins");

            System.Windows.MessageBox.Show($"Result: {result.result} checked: {result.dontChecked}");

            var id = Guid.NewGuid().ToString();
            var result2 = await ms.ShowAsyncWithDontShowAgain("This message should only appear once!", id, "Caption",
                HelpLinks.COMMANDS_ANALYZE, MessageImage.Stop, "Dont show that crap again");

            System.Windows.MessageBox.Show($"Result: {result2}");
            
            await ms.ShowAsyncWithDontShowAgain("If you checked the box in the last dialog, you shouldnt see this!", id, "Caption",
                HelpLinks.COMMANDS_ANALYZE, MessageImage.Stop, "Dont show that crap again");
            
            var id2 = Guid.NewGuid().ToString();
            var result3 = await ms.ShowRememberMyChoiceDialogAsync("Do you think I'm cool?", id2, "Caption",
                HelpLinks.COMMANDS_ANALYZE, MessageButton.YesNo, MessageImage.Stop, "Remember that!");

            System.Windows.MessageBox.Show($"Result: {result3}");
            
            result3 = await ms.ShowRememberMyChoiceDialogAsync("If you checked the box in the last dialog, you shouldnt see this!", id2, "Caption",
                HelpLinks.COMMANDS_ANALYZE, MessageButton.YesNo, MessageImage.Stop, "Dont show that crap again");
            
            System.Windows.MessageBox.Show($"Result: {result3}");

        }

        #endregion
    }
}