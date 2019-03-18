using System.Collections.Specialized;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportClearSelectedCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;

        public PresetExportClearSelectedCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetExport.ClearSelected, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
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