using System.Collections.Specialized;
using Catel;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportClearSelectedCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;

        public PresetExportClearSelectedCommandContainer(ICommandManager commandManager, IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetExport.ClearSelected, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;
            _vstService.SelectedPresets.CollectionChanged += OnSelectedPresetListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.SelectedPresets.Count > 0;
        }

        private void OnSelectedPresetListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override void Execute(object parameter)
        {
            _vstService.PresetExportList.RemoveItems(_vstService.SelectedPresets);
        }
    }
}