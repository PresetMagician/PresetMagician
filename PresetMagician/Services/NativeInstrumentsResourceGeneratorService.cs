using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Catel;
using Catel.Threading;
using ColorThiefDotNet;
using MethodTimer;
using PresetMagician.Helpers;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.Services.Interfaces;
using PresetMagician.VstHost.Util;
using SharedModels;
using SharedModels.NativeInstrumentsResources;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace PresetMagician.Services
{
    public class NativeInstrumentsResourceGeneratorService : INativeInstrumentsResourceGeneratorService
    {
        [Time]
        public void AutoGenerateResources(Plugin plugin, IRemoteVstService remoteVstService)
        {
            var generate = false;
            var imagesToSave = new List<ResourceImage>();
            foreach (var img in plugin.NativeInstrumentsResource.ResourceImages)
            {
                if (img.State.State != NativeInstrumentsResource.ResourceStates.Empty)
                {
                    continue;
                }

                generate = true;
                imagesToSave.Add(img);
            }

            var previousCategoriesState = plugin.NativeInstrumentsResource.CategoriesState.State;
            var previousColorState = plugin.NativeInstrumentsResource.ColorState.State;
            var previousShortNameState = plugin.NativeInstrumentsResource.ShortNamesState.State;

            if (plugin.NativeInstrumentsResource.CategoriesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty ||
                plugin.NativeInstrumentsResource.ShortNamesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty || plugin.NativeInstrumentsResource.ColorState.State ==
                NativeInstrumentsResource.ResourceStates.Empty || generate)
            {
                GenerateResources(plugin, remoteVstService);

                if (previousCategoriesState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    plugin.NativeInstrumentsResource.CategoriesState.ShouldSave = true;
                }

                if (previousColorState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    plugin.NativeInstrumentsResource.ColorState.ShouldSave = true;
                }

                if (previousShortNameState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    plugin.NativeInstrumentsResource.ShortNamesState.ShouldSave = true;
                }

                foreach (var img in imagesToSave)
                {
                    if (img.State.State == NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated)
                    {
                        img.State.ShouldSave = true;
                    }
                }

                plugin.NativeInstrumentsResource.Save(plugin);
            }
        }

        public bool ShouldCreateScreenshot(Plugin plugin)
        {
            var shouldCreateScreenshot = false;
            
            var niResource = plugin.NativeInstrumentsResource;
            
            if (niResource.ColorState.State == NativeInstrumentsResource.ResourceStates.Empty)
            {
                shouldCreateScreenshot = true;
            }
            
            foreach (var img in niResource.ResourceImages)
            {
                if (img.State.State == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    shouldCreateScreenshot = true;
                }
            }

            return shouldCreateScreenshot;
        }

        [Time]
        public void GenerateResources(Plugin plugin, IRemoteVstService remoteVstService, bool force = false)
        {
            Image bmp;
            var niResource = plugin.NativeInstrumentsResource;

            if (!ShouldCreateScreenshot(plugin) && !force)
            {
                return;
            }
            
            var data = remoteVstService.CreateScreenshot(plugin.Guid);

            if (data != null)
            {
                var ms = new MemoryStream();
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                bmp = Bitmap.FromStream(ms);
            }
            else
            {
                return;
            }

            var pluginName = plugin.PluginName;
            var pluginVendor = plugin.PluginVendor;

            niResource.VB_artwork.ReplaceFromStream(GetScaledBitmap(bmp, niResource.VB_artwork.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);
            niResource.MST_plugin.ReplaceFromStream(GetScaledBitmap(bmp, niResource.MST_plugin.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);
            niResource.MST_artwork.ReplaceFromStream(GetScaledBitmap(bmp, niResource.MST_artwork.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);


            niResource.Color.BackgroundColor = DetectBestColor((Bitmap) bmp);

            niResource.MST_logo.ReplaceFromStream(
                RenderBigLogo(pluginName, pluginVendor, niResource.MST_logo.TargetSize, 35, 12),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);

            niResource.VB_logo.ReplaceFromStream(
                RenderLogo(pluginName, niResource.VB_logo.TargetSize, 30, new Point(7, 2)),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);

            niResource.OSO_logo.ReplaceFromStream(
                RenderLogo(pluginName, niResource.OSO_logo.TargetSize, 43, new Point(7, 2)),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);
        }


        [Time]
        private static MemoryStream RenderLogo(string pluginName, Size targetSize, int fontSize, Point offset)
        {
            var text = new FormattedText(pluginName, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Segoe UI Light"), fontSize,
                Brushes.White, 1)
            {
                MaxTextWidth = targetSize.Width,
                MaxTextHeight = targetSize.Height,
                Trimming = TextTrimming.CharacterEllipsis
            };


            var d = new DrawingVisual();


            var d1 = d.RenderOpen();
            d1.DrawText(text, offset);
            d1.Close();
            var bmp2 = new RenderTargetBitmap(targetSize.Width,
                targetSize.Height, 96, 96, PixelFormats.Pbgra32);

            bmp2.Render(d);

            var pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(bmp2));

            var ms = new MemoryStream();
            pngImage.Save(ms);

            return ms;
        }

        [Time]
        private static MemoryStream RenderBigLogo(string pluginName, string vendorName, Size targetSize,
            int pluginFontSize, int vendorFontSize)
        {
            var center = new Point(0, 0);
            FormattedText text = new FormattedText(pluginName, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Segoe UI Bold"), pluginFontSize,
                Brushes.White, 1)
            {
                MaxTextWidth = targetSize.Width,
                TextAlignment = TextAlignment.Center,
                MaxLineCount = 2,
                Trimming = TextTrimming.CharacterEllipsis
            };

            var d = new DrawingVisual();


            var text2 = new FormattedText(vendorName, CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Segoe UI Light"), vendorFontSize,
                Brushes.White, 1)
            {
                MaxTextWidth = targetSize.Width, TextAlignment = TextAlignment.Center, MaxLineCount = 1
            };


            var d1 = d.RenderOpen();


            center.X = 0;
            center.Y = (double) targetSize.Height / 2 - text.Height / 2;
            d1.DrawText(text, center);
            center.Y = 20;
            d1.DrawText(text2, center);

            d1.Close();
            var bmp2 = new RenderTargetBitmap(targetSize.Width,
                targetSize.Height, 96, 96, PixelFormats.Pbgra32);

            bmp2.Render(d);

            var pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(bmp2));

            var ms = new MemoryStream();
            pngImage.Save(ms);

            return ms;
        }

        [Time]
        private static Color DetectBestColor(Bitmap bmp)
        {
            var colorThief = new ColorThief();
            var colors = colorThief.GetPalette(bmp, 8);

            var pluginColors = new List<PluginColor>();

            var maxPopulation = (from pluginColor in colors select pluginColor.Population).Max();

            foreach (var color in colors)
            {
                var col = ColorTranslator.FromHtml(color.Color.ToHexString());
                var populationWeight = 0.5;
                var saturationWeight = (float) Math.Tanh(col.GetSaturation());
                double darkWeight;

                var populationPercent = (double) color.Population / maxPopulation;

                if (populationPercent > 0.3)
                {
                    populationWeight = 0.3;
                }

                if (populationPercent < 0.1 && populationPercent > 0.01)
                {
                    populationWeight = 0.8;
                }

                if (populationPercent <= 0.01)
                {
                    populationWeight = 0.2;
                }

                if (color.IsDark)
                {
                    darkWeight = 1;
                }
                else
                {
                    darkWeight = 0.3;
                }

                if (col.GetBrightness() > 0.7)
                {
                    darkWeight = 0;
                }


                var weight = populationWeight + darkWeight + saturationWeight;

                pluginColors.Add(new PluginColor
                {
                    ColorCode = color.Color.ToHexString(),
                    Weight = weight
                });
            }


            var orderedColors = (from pluginColor in pluginColors
                orderby pluginColor.Weight descending
                select pluginColor).ToList();


            var bestColor = orderedColors.FirstOrDefault();
            return (Color) ColorConverter.ConvertFromString(
                bestColor.ColorCode);
        }

        private class PluginColor
        {
            public double Weight { get; set; }
            public string ColorCode { get; set; }
        }

        [Time]
        private static MemoryStream GetScaledBitmap(Image bitmap, Size targetSize)
        {
            var resizeSize = GetResizeSize(bitmap.Size, targetSize);

            var tmpBitmap = new Bitmap(resizeSize.Width, resizeSize.Height);
            var graph2 = Graphics.FromImage(tmpBitmap);
            graph2.SmoothingMode = SmoothingMode.HighQuality;
            graph2.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graph2.CompositingQuality = CompositingQuality.HighQuality;
            graph2.DrawImage(bitmap, 0, 0, resizeSize.Width, resizeSize.Height);

            var targetBmp = new Bitmap(targetSize.Width, targetSize.Height);
            var graph = Graphics.FromImage(targetBmp);
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graph.CompositingQuality = CompositingQuality.HighQuality;


            var leftOffset = (resizeSize.Width - targetSize.Width) / 2;
            var topOffset = (resizeSize.Height - targetSize.Height) / 2;

            var rect = new RectangleF(leftOffset, topOffset,
                targetSize.Width,
                targetSize.Height);

            graph.DrawImage(tmpBitmap, 0, 0, rect, GraphicsUnit.Pixel);

            var memoryStream = new MemoryStream();
            targetBmp.Save(memoryStream, ImageFormat.Png);
            return memoryStream;
        }

        [Time]
        private static Size GetResizeSize(Size input, Size targetSize)
        {
            var finalSize = new Size();

            var sourceHeight = (double) input.Height;
            var sourceWidth = (double) input.Width;
            var targetWidth = (double) targetSize.Width;
            var targetHeight = (double) targetSize.Height;


            var aspectRatio = sourceWidth / sourceHeight;

            var finalWidth = targetWidth; // 96
            var finalHeight = finalWidth / aspectRatio;

            if (finalHeight < targetHeight)
            {
                finalHeight = targetHeight;
                finalWidth = finalHeight * aspectRatio;
            }

            finalSize.Width = (int) finalWidth;
            finalSize.Height = (int) finalHeight;

            return finalSize;
        }
    }
}