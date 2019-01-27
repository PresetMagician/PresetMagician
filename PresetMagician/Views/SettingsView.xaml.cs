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
        }
    }
}