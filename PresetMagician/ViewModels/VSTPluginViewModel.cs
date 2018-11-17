using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Drachenkatze.PresetMagician.VSTHost.VST;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.GUI.ViewModels
{
    public class VSTPluginViewModel : INotifyPropertyChanged
    {
        public VSTPluginViewModel()
        {
            this.VstPlugins = new ObservableCollection<VSTPlugin>();
        }

        public ObservableCollection<VSTPlugin> VstPlugins { get; set; }

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