using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Be.Windows.Forms;
using Catel.Windows;
using PresetMagicianShell.Models;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaction logic for VstPluginChunkView.xaml
    /// </summary>
    public partial class VstPluginChunkView
    {
        public VstPluginChunkView()
        {
            InitializeComponent();
        }

        public VstPluginChunkView(VstPluginChunkViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Open Bank with HxD", "OpenWithHxDBank"));
            AddCustomButton(new DataWindowButton("Open Preset with HxD", "OpenWithHxDPreset"));

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();

            var provider = new DynamicByteProvider(viewModel.Plugin.ChunkBankMemoryStream.ToArray());
            PluginBankChunkControl.ByteProvider = provider;

            var provider2 = new DynamicByteProvider(viewModel.Plugin.ChunkPresetMemoryStream.ToArray());
            PluginPresetChunkControl.ByteProvider = provider2;

        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
        }
    }
}
