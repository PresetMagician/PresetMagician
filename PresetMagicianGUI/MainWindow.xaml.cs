using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.Configuration;
using System.Collections.Specialized;
using MahApps.Metro.Controls;
using PresetMagicianGUI.ViewModels;

namespace PresetMagicianGUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new VSTPathViewModel();

            App.setStatusBar("1.21 Gigawatts ready to engage.");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VSTPluginListControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
