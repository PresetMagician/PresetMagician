using System.Windows;

namespace PresetMagician.VstHost
{
    /// <summary>
    /// Interaction logic for PluginWindow.xaml
    /// </summary>
    public partial class PluginWindow : Window
    {
        public PluginWindow()
        {
            InitializeComponent();
        }

        public PluginWindow(int width, int height)
        {
            InitializeComponent();
            PreviewAirspaceDecorator.Width = width;
            PreviewAirspaceDecorator.Height = height;
        }
    }
}