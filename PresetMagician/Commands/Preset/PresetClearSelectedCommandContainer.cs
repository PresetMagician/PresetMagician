using System.Collections.Specialized;
using Catel;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetClearSelectedCommandContainer : CommandContainerBase
    {
        private readonly IVstService _vstService;

        public PresetClearSelectedCommandContainer(ICommandManager commandManager, IVstService vstService)
            : base(Commands.Preset.ClearSelected, commandManager)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;
            _vstService.SelectedPresets.CollectionChanged += OnSelectedPresetListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.SelectedPresets.Count > 0;
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