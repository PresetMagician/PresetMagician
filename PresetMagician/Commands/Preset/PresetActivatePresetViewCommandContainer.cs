using Catel.MVVM;
using PresetMagician.Helpers;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetActivatePresetViewCommandContainer : CommandContainerBase
    {
        public PresetActivatePresetViewCommandContainer(ICommandManager commandManager)
            : base(Commands.Preset.ActivatePresetView, commandManager)
        {
        }

        protected override void Execute(object parameter)
        {
            AvalonDockHelper.ActivateDocument<PresetExportListViewModel>();
            base.Execute(parameter);
        }
    }
}