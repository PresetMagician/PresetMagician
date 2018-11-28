using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel;
using Catel.Configuration;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using Orchestra.ViewModels;

namespace PresetMagicianShell.ViewModels
{
    public class RibbonViewModel : ViewModelBase
    {
        #region Fields

        private readonly IUIVisualizerService _uiVisualizerService;

        #endregion Fields

        #region Constructors

        public RibbonViewModel(
            IUIVisualizerService uiVisualizerService
           )
        {
            Argument.IsNotNull(() => uiVisualizerService);
            _uiVisualizerService = uiVisualizerService;
            ShowKeyboardMappings = new TaskCommand(OnShowKeyboardMappingsExecuteAsync);
            Title = AssemblyHelper.GetEntryAssembly().Title();
        }

        public TaskCommand ShowKeyboardMappings { get; private set; }

        private async Task OnShowKeyboardMappingsExecuteAsync()
        {
            await _uiVisualizerService.ShowDialogAsync<KeyboardMappingsCustomizationViewModel>();
        }

        #endregion Constructors
    }
}