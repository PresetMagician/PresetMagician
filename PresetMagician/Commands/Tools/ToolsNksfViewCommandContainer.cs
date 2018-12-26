using Catel.MVVM;
using Catel.Services;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsNksfViewCommandContainer : AbstractOpenDialogCommandContainer
    {
        public ToolsNksfViewCommandContainer(ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory)
            : base(Commands.Tools.NksfView, nameof(NKSFViewModel), commandManager, uiVisualizerService, viewModelFactory)
        {
        }
    }
}