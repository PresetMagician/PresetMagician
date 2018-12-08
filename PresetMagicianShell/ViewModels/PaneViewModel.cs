using System.Windows.Controls;
using System.Windows.Media;
using Catel.MVVM;

namespace PresetMagicianShell.ViewModels
{
    public class PaneViewModel : ViewModelBase
    {
       
        #region Title

        private string _title;

        public override string Title
        {
            get { return _title; }
            protected set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        public string MyTitle { get; set; } = "yoooo";

        #endregion

        

        #region ContentId

        private string _contentId = null;

        public string ContentId
        {
            get { return _contentId; }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    RaisePropertyChanged("ContentId");
                }
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected = false;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region IsActive

        private bool _isActive = false;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        #endregion
    }
}