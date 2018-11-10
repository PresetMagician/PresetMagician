using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace PresetMagicianGUI.ViewModels
{
    public enum ListViewMode
    {
        Standard,
        Tile
    }

    public class VSTPathViewModel : INotifyPropertyChanged
    {
        public VSTPathViewModel()
        {
            this.VstPaths = new ObservableCollection<DirectoryInfo>();
        }

        public ObservableCollection<DirectoryInfo> VstPaths { get; set; }

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
