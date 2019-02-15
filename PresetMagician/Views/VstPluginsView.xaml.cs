using System.Windows.Controls;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for VstPluginListControl.xaml
    /// </summary>
    public partial class VstPluginsView
    {
        public VstPluginsView()
        {
            InitializeComponent();
        }

        private void VstPluginList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (VstPluginList.SelectedItem != null)
            {
                VstPluginList.ScrollIntoView(VstPluginList.SelectedItem);
            }*/
        }
    }
}