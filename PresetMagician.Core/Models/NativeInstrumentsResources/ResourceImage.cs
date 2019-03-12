using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Anotar.Catel;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace PresetMagician.Core.Models.NativeInstrumentsResources
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
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.Freeze();
            Image = bitmapImage;

        }

        private ResourceImage()
        {
           
        }


        public BitmapImage Image { get; set; }

        private byte[] GetImageData()
        {
            if (Image == null) return null;
            var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            BitmapFrame frame = BitmapFrame.Create(Image);
            encoder.Frames.Add(frame);
            encoder.Save(ms);
            return ms.ToArray();
        }

        protected override void OnSerializing()
        {
            if (Image == null) return;

            _serializedImage = GetImageData();
            base.OnSerializing();
        }

        protected override void OnDeserialized()
        {
            base.OnDeserialized();
            if (_serializedImage != null ) {
                using (MemoryStream ms = new MemoryStream(_serializedImage))
                {
                    var tmpImage = new BitmapImage();
                    tmpImage.BeginInit();
                    tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    tmpImage.BaseUri = null;
                    tmpImage.StreamSource = ms;
                    tmpImage.EndInit();
                    tmpImage.Freeze();

                    Image = tmpImage;
                }
            }

            
        }

        [IncludeInSerialization]
        private byte[] _serializedImage { get; set; }
      

        public Size TargetSize { get; set; }
        public string Filename { get; set; }

        public void ReplaceFromFile(string fileName,
            NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromDisk)
        {
            State.State = resourceState;
            var tmpImage = new BitmapImage();
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                tmpImage.BeginInit();
                tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpImage.BaseUri = null;
                tmpImage.StreamSource = fs;
                tmpImage.EndInit();
                tmpImage.Freeze();
            }
          
            Image = tmpImage;
        }

        public void ReplaceFromBase64(string base64,
            NativeInstrumentsResource.ResourceStates resourceState = NativeInstrumentsResource.ResourceStates.FromWeb)
        {
            State.State = resourceState;
           
            var tmpImage = new BitmapImage();
            
            using (var ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                tmpImage.BeginInit();
                tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpImage.BaseUri = null;
                tmpImage.StreamSource = ms;
                tmpImage.EndInit();
                tmpImage.Freeze();
            }

            Image = tmpImage;
        }

        public void ReplaceFromStream(MemoryStream memoryStream, NativeInstrumentsResource.ResourceStates resourceState)
        {
            State.State = resourceState;
           
            var tmpImage = new BitmapImage();
            tmpImage.BeginInit();
            tmpImage.CacheOption = BitmapCacheOption.OnLoad;
            tmpImage.BaseUri = null;
            tmpImage.StreamSource = memoryStream;
            tmpImage.EndInit();
            tmpImage.Freeze();

            Image = tmpImage;
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(GetImageData());
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

            File.WriteAllBytes(fullFile, GetImageData());
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