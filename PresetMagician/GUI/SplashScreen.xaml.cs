using Svg;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Drachenkatze.PresetMagician.GUI.GUI
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            displaySplashImage();
        }

        public void setSplashMessage(String message)
        {
            splashMessage.Text = message;
        }

        public System.Windows.Size getPixelDimensions()
        {
            System.Windows.Size s = new System.Windows.Size();

            PresentationSource presentationsource = PresentationSource.FromVisual(this);

            s.Width = ActualWidth * presentationsource.CompositionTarget.TransformToDevice.M11;
            s.Height = ActualHeight * presentationsource.CompositionTarget.TransformToDevice.M22;
            return s;
        }

        public void displaySplashImage()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Encoding.UTF8.GetString(Properties.Resources.Splashscreen));

            SvgDocument svgDoc = SvgDocument.Open(xmlDoc);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            var size = getPixelDimensions();

            var bitmap = svgDoc.Draw((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            splashImage.Source = bi;
        }
    }
}