using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace PresetMagicianGUI
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
            this.SimpleItems = new ObservableCollection<DirectoryInfo>(new[]
            {
        new DirectoryInfo(@"C:\ProgramData")
      });
        }

        public ObservableCollection<DirectoryInfo> SimpleItems { get; set; }

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
