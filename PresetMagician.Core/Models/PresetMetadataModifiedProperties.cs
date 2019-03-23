using System.ComponentModel;
using Catel.Data;

namespace PresetMagician.Core.Models
{
    public class PresetMetadataModifiedProperties: INotifyPropertyChanged
    {
        public bool IsAuthorModified { get; set; }
        public bool IsCommentModified { get; set; }
        public bool IsPresetNameModified { get; set; }
        public bool IsBankPathModified { get; set; }
        public bool IsCharacteristicsModified { get; set; }
        public bool IsTypesModified { get; set; }

        public bool IsModified()
        {
            return IsAuthorModified || IsCommentModified || IsPresetNameModified || IsBankPathModified
                   || IsCharacteristicsModified || IsTypesModified;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            var x = new AdvancedPropertyChangedEventArgs(this, propertyName, before, after);
            PropertyChanged?.Invoke(this, x);
        }
    }
}