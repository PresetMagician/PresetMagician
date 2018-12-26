using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagicianShell.Helpers;
using PresetMagicianShell.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PresetActivatePresetViewCommandContainer : CommandContainerBase
    {
        public PresetActivatePresetViewCommandContainer(ICommandManager commandManager)
            : base(Commands.Preset.ActivatePresetView, commandManager)
        {
        }

        protected override void Execute (object parameter)
        {
            AvalonDockHelper.ActivateDocument<PresetExportListViewModel>();
            base.Execute(parameter);
        }
    }
}   