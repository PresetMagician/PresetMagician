using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class PresetBankView
    {
        public PresetBankView(PresetBankViewModel viewModel)
            : base(viewModel, DataWindowMode.OkCancel)
        {
            InitializeComponent();
        }
    }
}