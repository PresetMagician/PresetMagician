using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Drachenkatze.PresetMagician.VSTHost.VST;
using UserControl = System.Windows.Controls.UserControl;

namespace Drachenkatze.PresetMagician.GUI.Controls
{
    public partial class VstFolderListControl : UserControl
    {
        public VstFolderListControl()
        {
            InitializeComponent();

            this.DataContext = App.vstPaths;
        }

        private void DefaultVSTPath_Click(object sender, RoutedEventArgs e)
        {
            foreach (DirectoryInfo i in VstPathScanner.getCommonVSTPluginDirectories())
            {
                if ((from path in App.vstPaths.VstPaths where path.FullName == i.FullName select path.FullName).Count() == 0)
                {
                    App.vstPaths.VstPaths.Add(i);
                }
            }
        }

        private void RemoveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (VstFolderList.SelectedItems.Count == 0)
            {
                return;
            }

            ObservableCollection<DirectoryInfo> toRemove = new ObservableCollection<DirectoryInfo>();

            foreach (DirectoryInfo i in VstFolderList.SelectedItems)
            {
                toRemove.Add(i);
            }

            foreach (DirectoryInfo i in toRemove)
            {
                App.vstPaths.VstPaths.Remove(i);
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
                    App.vstPaths.VstPaths.Add(path);
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (VstFolderList.SelectedItems.Count == 0)
            {
                RemoveFolderButton.IsEnabled = false;
            }
            else
            {
                RemoveFolderButton.IsEnabled = true;
            }
        }
    }
}