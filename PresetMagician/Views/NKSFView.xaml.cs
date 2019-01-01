using System.ComponentModel;
using System.Windows;
using Be.Windows.Forms;
using Catel.Data;
using Catel.IoC;
using Catel.IO;
using Catel.Windows;
using Orchestra.Services;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    /// <summary>
    ///     Interaction logic for NKSFViewer.xaml
    /// </summary>
    public partial class NKSFView
    {
        /// <summary>
        /// </summary>
        public NKSFView() : this(null)
        {
        }

        public NKSFView(NKSFViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Open NKSF File", "OpenNKSFFile"));
            AddCustomButton(new DataWindowButton("Open Chunk with HxD", "OpenWithHxD"));

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();

        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PluginChunk")
            {
                var s = (NKSFViewModel) sender;

                var provider = new DynamicByteProvider(s.PluginChunk.ToArray());
                PluginChunkControl.ByteProvider = provider;

            }
        }

    }
}