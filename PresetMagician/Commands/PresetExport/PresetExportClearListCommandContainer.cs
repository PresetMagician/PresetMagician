using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportClearListCommandContainer : ApplicationNotBusyCommandContainer
    {
        public PresetExportClearListCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PresetExport.ClearList, commandManager, serviceLocator)
        {
        }

        protected override void Execute(object parameter)
        {
            _globalFrontendService.SelectedPresets.Clear();
            _globalFrontendService.PresetExportList.Clear();
        }
    }
}