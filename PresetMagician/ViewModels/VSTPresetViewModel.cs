using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Drachenkatze.PresetMagician.GUI.Models;
using Drachenkatze.PresetMagician.VSTHost.VST;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.GUI.ViewModels
{
    public class VSTPresetViewModel : INotifyPropertyChanged
    {
        public VSTPresetViewModel()
        {
            this.VstPresets = new ObservableCollection<Preset>();
        }

        public ObservableCollection<Preset> VstPresets { get; set; }

        private ListViewMode _listViewMode;

        public ListViewMode ListViewMode
        {
            get { return _listViewMode; }
            set
            {
                if (value == _listViewMode) return;
                _listViewMode = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}