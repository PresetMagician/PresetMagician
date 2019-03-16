using System.Windows.Controls;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class CharacteristicView 
    {
        public CharacteristicView(CharacteristicViewModel viewModel): base(viewModel, DataWindowMode.OkCancel) {
            InitializeComponent();
            
        }
    }
}
