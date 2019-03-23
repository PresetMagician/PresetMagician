using System.Collections.Generic;
using Catel.Data;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class PreviewNotePlayerEditorViewModel:ViewModelBase
    {
        private readonly GlobalService _globalService;
        
        public PreviewNotePlayerEditorViewModel(GlobalService globalService)
        {
            _globalService = globalService;
            
            PreviewNotePlayers = globalService.PreviewNotePlayers;
            
            AddPreviewPlayer = new Command(OnAddPreviewPlayerExecute);
            RemovePreviewPlayer = new Command(OnRemovePreviewPlayerExecute, RemovePreviewPlayerCanExecute);
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPreviewNotePlayer))
            {
                RemovePreviewPlayer.RaiseCanExecuteChanged();
            }
            
            base.OnPropertyChanged(e);
        }

        public List<PreviewNotePlayer> PreviewNotePlayers { get; }
        public PreviewNotePlayer SelectedPreviewNotePlayer { get; set; }
        
        public Command AddPreviewPlayer { get; }

        private void OnAddPreviewPlayerExecute ()
        {
           PreviewNotePlayers.Add(new PreviewNotePlayer { Name = "New Player"});
        }
        
        public Command RemovePreviewPlayer { get; }

        private void OnRemovePreviewPlayerExecute ()
        {
            
            PreviewNotePlayers.Remove(SelectedPreviewNotePlayer);
        }
        
        private bool RemovePreviewPlayerCanExecute ()
        {
            return SelectedPreviewNotePlayer != null && SelectedPreviewNotePlayer.PreviewNotePlayerId != "default";
        }
    }
}