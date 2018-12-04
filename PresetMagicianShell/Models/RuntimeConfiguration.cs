using System;
using System.Collections.ObjectModel;
using Catel.Data;

namespace PresetMagicianShell.Models
{
    [Serializable]
    public class RuntimeConfiguration: ModelBase
    {
        public ObservableCollection<String> VstDirectories
        {
            get { return GetValue<ObservableCollection<String>>(VstDirectoriesProperty); }
            set { SetValue(VstDirectoriesProperty, value); }
        }

        public static readonly PropertyData VstDirectoriesProperty = RegisterProperty("VstDirectories", typeof(ObservableCollection<String>), () => new ObservableCollection<String>());

    }
}