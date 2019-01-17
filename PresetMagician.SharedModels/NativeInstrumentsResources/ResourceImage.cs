using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Anotar.Catel;
using Catel.Data;
using PresetMagician.Models.NativeInstrumentsResources;
using Size = System.Drawing.Size;

namespace SharedModels.NativeInstrumentsResources
{
    public class ResourceImage : ModelBase
    {
        private NativeInstrumentsResource.ResourceStates _resourceState;

        public NativeInstrumentsResource.ResourceStates ResourceState
        {
            get => _resourceState;
            set
            {
                switch (value)
                {
                    case NativeInstrumentsResource.ResourceStates.Empty:
                    case NativeInstrumentsResource.ResourceStates.FromDisk:
                        ShouldSaveImage = false;
                        break;
                    case NativeInstrumentsResource.ResourceStates.FromWeb:
                    case NativeInstrumentsResource.ResourceStates.UserModified:
                    case NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated:
                        ShouldSaveImage = true;
                        break;
                }

                _resourceState = value;
            }
        }

        public bool ShouldSaveImage { get; set; }

        public ResourceImage(int width, int height, string fileName)
        {
            var targetSize = TargetSize;
            targetSize.Width = width;
            targetSize.Height = height;
            TargetSize = targetSize;
            Filename = fileName;
        }

        public BitmapImage Image { get; set; } = new BitmapImage();
        public MemoryStream ImageStream { get; set; } = new MemoryStream();
        public Size TargetSize { get; set; }
        public string Filename { get; set; }

        public void ReplaceFromFile(string fileName, NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromDisk)
        {
            ResourceState = resourceState;
            ImageStream.SetLength(0);

            var bytes = File.ReadAllBytes(fileName);
            ImageStream.Write(bytes, 0, bytes.Length);
            ImageStream.Seek(0, SeekOrigin.Begin);

            Image = new BitmapImage();
            Image.BeginInit();
            Image.BaseUri = null;
            Image.StreamSource = ImageStream;
            Image.EndInit();
            Image.Freeze();
        }

        public void ReplaceFromBase64(string base64, NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromWeb)
        {
            ResourceState = resourceState;
            ImageStream.SetLength(0);

            var bytes = Convert.FromBase64String(base64);
            ImageStream.Write(bytes, 0, bytes.Length);
            ImageStream.Seek(0, SeekOrigin.Begin);

            Image = new BitmapImage();
            Image.BeginInit();
            Image.BaseUri = null;
            Image.StreamSource = ImageStream;
            Image.EndInit();
            Image.Freeze();
        }

        public void ReplaceFromStream(MemoryStream memoryStream, NativeInstrumentsResource.ResourceStates resourceState)
        {
            ResourceState = resourceState;
            ImageStream.SetLength(0);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(ImageStream);

            ImageStream.Seek(0, SeekOrigin.Begin);

            Image = new BitmapImage();
            Image.BeginInit();
            Image.BaseUri = null;
            Image.StreamSource = ImageStream;
            Image.EndInit();
            Image.Freeze();
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(ImageStream.ToArray());
        }

        public void Save(string baseDirectory)
        {
            var fullFile = Path.Combine(baseDirectory, Filename);
            if (!ShouldSaveImage)
            {
                LogTo.Debug($"Not saving with state {nameof(ResourceState)} for file {Filename} (full path {fullFile})");
                return;
            }

            File.WriteAllBytes(fullFile, ImageStream.ToArray());
            ResourceState = NativeInstrumentsResource.ResourceStates.FromDisk;
        }

        public void Load(string baseDirectory)
        {
            var fullPath = Path.Combine(baseDirectory, Filename);

            if (File.Exists(fullPath))
            {
                ReplaceFromFile(fullPath);
            }
        }
    }
}