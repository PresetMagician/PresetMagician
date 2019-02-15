using Catel.MVVM;
using PresetMagician.Helpers;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportActivatePresetViewCommandContainer : CommandContainerBase
    {
        public PresetExportActivatePresetViewCommandContainer(ICommandManager commandManager)
            : base(Commands.PresetExport.ActivatePresetView, commandManager)
        {
        }

        protected override void Execute(object parameter)
        {
            AvalonDockHelper.ActivateDocument<PresetExportListViewModel>();
            base.Execute(parameter);
        }
    }
}