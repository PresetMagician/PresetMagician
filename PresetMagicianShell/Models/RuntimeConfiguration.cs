using System;
using System.Collections.ObjectModel;
using Catel.Data;
using PresetMagicianShell.Models.Settings;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagicianShell.Models
{
    [Serializable]
    public class RuntimeConfiguration: ModelBase
    {
        public ObservableCollection<VstDirectory> VstDirectories
        {
            get => GetValue<ObservableCollection<VstDirectory>>(VstDirectoriesProperty);
            set => SetValue(VstDirectoriesProperty, value);
        }

        public static readonly PropertyData VstDirectoriesProperty = RegisterProperty("VstDirectories", typeof(ObservableCollection<VstDirectory>), () => new ObservableCollection<VstDirectory>());
        
    }
}