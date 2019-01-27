using System.Windows;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using ThemeHelper2 = Catel.ThemeHelper;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ThemeControlsWindow
    {
        public ThemeControlsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox mb = new MessageBox();
            mb.Caption = "Caption";
            mb.Text = "Text";
            mb.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }
    }
}