using System.Windows;
using Catel.MVVM;

namespace PresetMagician.ViewModels
{
    public class ProgressWindowViewModel : ViewModelBase
    {
        public string Title { get; set; }

        public void SetOwnerWindow(Window window)
        {
            Left = window.Left + (window.ActualWidth / 2 - Width / 2);
            Top = window.Top + (window.ActualHeight / 2 - Height / 2);
        }

        public double Left { get; private set; }
        public double Top { get; private set; }
        public double Width { get; set; } = 400;
        public double Height { get; set; } = 100;
    }
}