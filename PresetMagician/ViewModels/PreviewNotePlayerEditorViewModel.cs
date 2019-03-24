using System.Collections.Generic;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class PreviewNotePlayerEditorViewModel : ViewModelBase
    {
        private readonly GlobalService _globalService;

        public PreviewNotePlayerEditorViewModel(GlobalService globalService)
        {
            _globalService = globalService;

            PreviewNotePlayers = globalService.PreviewNotePlayers;

            AddPreviewPlayer = new Command(OnAddPreviewPlayerExecute);
            RemovePreviewPlayer = new Command(OnRemovePreviewPlayerExecute, RemovePreviewPlayerCanExecute);

            AddNote = new Command(OnAddNoteExecute);
            RemoveNote = new Command(OnRemoveNoteExecute, RemoveNoteCanExecute);
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPreviewNotePlayer))
            {
                RemovePreviewPlayer.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(SelectedPreviewNote))
            {
                RemoveNote.RaiseCanExecuteChanged();
            }

            base.OnPropertyChanged(e);
        }

        public FastObservableCollection<PreviewNotePlayer> PreviewNotePlayers { get; }
        public PreviewNotePlayer SelectedPreviewNotePlayer { get; set; }
        public PreviewNote SelectedPreviewNote { get; set; }

        public Command AddPreviewPlayer { get; }

        private void OnAddPreviewPlayerExecute()
        {
            var player = new PreviewNotePlayer {Name = "New Player"};
            player.PreviewNotes.Add(new PreviewNote { Start = 0, Duration = 1, NoteNumber = 48});
            PreviewNotePlayers.Add(player);
        }

        public Command RemovePreviewPlayer { get; }

        private void OnRemovePreviewPlayerExecute()
        {
            PreviewNotePlayers.Remove(SelectedPreviewNotePlayer);
        }

        private bool RemovePreviewPlayerCanExecute()
        {
            return SelectedPreviewNotePlayer != null && SelectedPreviewNotePlayer.PreviewNotePlayerId != "default";
        }

        public Command AddNote { get; }

        private void OnAddNoteExecute()
        {
            SelectedPreviewNotePlayer.PreviewNotes.Add(new PreviewNote { Start = 0, Duration = 1, NoteNumber = 48});
        }

        public Command RemoveNote { get; }

        private void OnRemoveNoteExecute()
        {
            SelectedPreviewNotePlayer.PreviewNotes.Remove(SelectedPreviewNote);
        }

        private bool RemoveNoteCanExecute()
        {
            return SelectedPreviewNote != null && SelectedPreviewNotePlayer.PreviewNotes.Count > 1;
        }
    }
}