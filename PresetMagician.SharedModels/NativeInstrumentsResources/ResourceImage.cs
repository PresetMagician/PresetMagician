using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Anotar.Catel;
using Catel.Data;
using PresetMagician.Models.NativeInstrumentsResources;

namespace SharedModels.NativeInstrumentsResources
{
    public class ResourceImage : ModelBase
    {
        public ResourceState State { get; } = new ResourceState();


        public ResourceImage(int width, int height, string fileName)
        {
            var targetSize = TargetSize;
            targetSize.Width = width;
            targetSize.Height = height;
            TargetSize = targetSize;
            Filename = fileName;
            var bitmapImage = new BitmapImage(
                new Uri("pack://application:,,,/PresetMagician.SharedModels;component/Resources/Images/empty.png"));
            bitmapImage.Freeze();
            Image = bitmapImage;
        }


        public BitmapImage Image { get; set; } 

        public MemoryStream ImageStream { get; set; } = new MemoryStream();
        public Size TargetSize { get; set; }
        public string Filename { get; set; }

        public void ReplaceFromFile(string fileName,
            NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromDisk)
        {
            State.State = resourceState;
            ImageStream.SetLength(0);

            var bytes = File.ReadAllBytes(fileName);
            ImageStream.Write(bytes, 0, bytes.Length);
            ImageStream.Seek(0, SeekOrigin.Begin);

            var tmpImage = new BitmapImage();
            tmpImage.BeginInit();
            tmpImage.BaseUri = null;
            tmpImage.StreamSource = ImageStream;
            tmpImage.EndInit();
            tmpImage.Freeze();

            Image = tmpImage;
        }

        public void ReplaceFromBase64(string base64,
            NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromWeb)
        {
            State.State = resourceState;
            ImageStream.SetLength(0);

            var bytes = Convert.FromBase64String(base64);
            ImageStream.Write(bytes, 0, bytes.Length);
            ImageStream.Seek(0, SeekOrigin.Begin);

            var tmpImage = new BitmapImage();
            tmpImage.BeginInit();
            tmpImage.BaseUri = null;
            tmpImage.StreamSource = ImageStream;
            tmpImage.EndInit();
            tmpImage.Freeze();

            Image = tmpImage;
        }

        public void ReplaceFromStream(MemoryStream memoryStream, NativeInstrumentsResource.ResourceStates resourceState)
        {
            State.State = resourceState;
            ImageStream.SetLength(0);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(ImageStream);

            ImageStream.Seek(0, SeekOrigin.Begin);

            var tmpImage = new BitmapImage();
            tmpImage.BeginInit();
            tmpImage.BaseUri = null;
            tmpImage.StreamSource = ImageStream;
            tmpImage.EndInit();
            tmpImage.Freeze();

            Image = tmpImage;
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(ImageStream.ToArray());
        }

        public void Save(string baseDirectory)
        {
            var fullFile = Path.Combine(baseDirectory, Filename);
            if (!State.ShouldSave)
            {
                LogTo.Debug(
                    $"Not saving with state {State.State.ToString()} for file {Filename} (full path {fullFile})");
                return;
            }

            File.WriteAllBytes(fullFile, ImageStream.ToArray());
            State.State = NativeInstrumentsResource.ResourceStates.FromDisk;
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