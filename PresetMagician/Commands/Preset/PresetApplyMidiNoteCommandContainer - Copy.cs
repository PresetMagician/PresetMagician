using System.Collections.Specialized;
using System.Diagnostics;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetApplyMidiNoteCommandContainer : CommandContainerBase
    {
        private readonly IVstService _vstService;

        public PresetApplyMidiNoteCommandContainer(ICommandManager commandManager, IVstService vstService)
            : base(Commands.Preset.ApplyMidiNote, commandManager)
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
            foreach (var preset in _vstService.SelectedPresets)
            {
                preset.PreviewNote.FullNoteName = (string)parameter;
            }
        }
    }
}