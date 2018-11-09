using PresetMagician.VST;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace PresetMagicianGUI
{
    /// <summary>
    /// Interaktionslogik für Page1.xaml
    /// </summary>
    public partial class VSTFolderListControl : System.Windows.Controls.UserControl
    {
        private VSTPathViewModel pathViewModel;

        public VSTFolderListControl()
        {
            InitializeComponent();
            pathViewModel = new VSTPathViewModel();
            this.DataContext = pathViewModel;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VSTFolderList.SelectedItems.Count == 0)
            {
                RemoveFolderButton.IsEnabled = false;
            }
            else
            {
                RemoveFolderButton.IsEnabled = true;
            }
        }

        private void DefaultVSTPath_Click(object sender, RoutedEventArgs e)
        {
            foreach (DirectoryInfo i in VstPathScanner.getCommonVSTPluginDirectories())
            {
                if ((from path in this.pathViewModel.SimpleItems where path.FullName == i.FullName select path.FullName).Count() == 0)
                {
                    this.pathViewModel.SimpleItems.Add(i);
                }
            }
        }

        private void RemoveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (VSTFolderList.SelectedItems.Count == 0)
            {
                return;
            }

            ObservableCollection<DirectoryInfo> toRemove = new ObservableCollection<DirectoryInfo>();

            foreach (DirectoryInfo i in VSTFolderList.SelectedItems)
            {
                toRemove.Add(i);
            }

            foreach (DirectoryInfo i in toRemove)
            {
                this.pathViewModel.SimpleItems.Remove(i);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    DirectoryInfo path = new DirectoryInfo(dialog.SelectedPath);
                    this.pathViewModel.SimpleItems.Add(path);
                }
            }
        }
    }
}