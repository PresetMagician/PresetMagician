using System.Diagnostics;
using Catel.Windows;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;
using PresetMagicianShell.ViewModels;
using Syroot.Windows.IO;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaction logic for NKSFViewer.xaml
    /// </summary>
    public partial class NKSFView : DataWindow
    {
        public NKSFView() : this(null)
        {
        }

        public NKSFView(NKSFViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Open NKSF File", "OpenNKSFFile"));
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PluginChunk")
            {
                var s = (NKSFViewModel)sender;
                PluginChunkControl.Stream = s.PluginChunk;
            }
        }

        private void SavePresetChunk_Click(object sender, RoutedEventArgs e)
        {
            /*    if (PluginChunk.Stream == null)
                {
                    System.Windows.MessageBox.Show("No data loaded");
                    return;
                }
                var fileContent = string.Empty;
                var filePath = string.Empty;

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

                    saveFileDialog.InitialDirectory = downloadsPath;
                    saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (var fileStream = new FileStream(filePath = saveFileDialog.FileName, FileMode.Create))
                        {
                            PluginChunk.Stream.Seek(0, SeekOrigin.Begin);
                            PluginChunk.Stream.CopyTo(fileStream);
                        }
                    }
                }*/
        }
    }
}