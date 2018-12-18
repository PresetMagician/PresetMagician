using Catel.MVVM;
using Catel.Services;
using PresetMagicianShell.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
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