using System.Collections.Specialized;
using Catel.IoC;
using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportClearSelectedCommandContainer : ApplicationNotBusyCommandContainer
    {
        public PresetExportClearSelectedCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PresetExport.ClearSelected, commandManager, serviceLocator)
        {
            _globalFrontendService.SelectedPresets.CollectionChanged += OnSelectedPresetListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPresets.Count > 0;
        }

        private void OnSelectedPresetListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override void Execute(object parameter)
        {
            _globalFrontendService.PresetExportList.RemoveItems(_globalFrontendService.SelectedPresets);
        }
    }
}