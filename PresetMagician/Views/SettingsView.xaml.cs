using System.Windows.Controls;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class SettingsView
    {
        public SettingsView(SettingsViewModel viewModel)
            : base(viewModel, DataWindowMode.OkCancel)
        {
            InitializeComponent();

            foreach (var item in TabControl.Items)
            {
                var x = item as TabItem;

                if ((string) x.Header == viewModel.SelectedTabTitle)
                {
                    x.IsSelected = true;
                }
            }
        }
    }
}