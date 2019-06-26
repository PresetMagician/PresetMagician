using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ColorThiefDotNet;
using MethodTimer;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.NativeInstrumentsResources;
using PresetMagician.Services.Interfaces;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using NiResourceColor = PresetMagician.Core.Models.NativeInstrumentsResources.Color;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace PresetMagician.Services
{
    public class NativeInstrumentsResourceGeneratorService : INativeInstrumentsResourceGeneratorService
    {
        public bool NeedToGenerateResources(IRemotePluginInstance pluginInstance)
        {
            foreach (var img in pluginInstance.Plugin.NativeInstrumentsResource.ResourceImages)
            {
                if (img.State.State == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    return true;
                }
            }


            if (pluginInstance.Plugin.NativeInstrumentsResource.CategoriesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty ||
                pluginInstance.Plugin.NativeInstrumentsResource.ShortNamesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty ||
                pluginInstance.Plugin.NativeInstrumentsResource.ColorState.State ==
                NativeInstrumentsResource.ResourceStates.Empty)
            {
                return true;
            }

            return false;
        }

        [Time]
        public void AutoGenerateResources(IRemotePluginInstance pluginInstance)
        {
            var generate = false;
            var imagesToSave = new List<ResourceImage>();
            foreach (var img in pluginInstance.Plugin.NativeInstrumentsResource.ResourceImages)
            {
                if (img.State.State != NativeInstrumentsResource.ResourceStates.Empty)
                {
                    continue;
                }

                generate = true;
                imagesToSave.Add(img);
            }

            var previousCategoriesState = pluginInstance.Plugin.NativeInstrumentsResource.CategoriesState.State;
            var previousColorState = pluginInstance.Plugin.NativeInstrumentsResource.ColorState.State;
            var previousShortNameState = pluginInstance.Plugin.NativeInstrumentsResource.ShortNamesState.State;

            if (pluginInstance.Plugin.NativeInstrumentsResource.CategoriesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty ||
                pluginInstance.Plugin.NativeInstrumentsResource.ShortNamesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty ||
                pluginInstance.Plugin.NativeInstrumentsResource.ColorState.State ==
                NativeInstrumentsResource.ResourceStates.Empty || generate)
            {
                GenerateResources(pluginInstance);

                if (previousCategoriesState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    pluginInstance.Plugin.NativeInstrumentsResource.CategoriesState.ShouldSave = true;
                }

                if (previousColorState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    pluginInstance.Plugin.NativeInstrumentsResource.ColorState.ShouldSave = true;
                }

                if (previousShortNameState == NativeInstrumentsResource.ResourceStates.Empty)
                {
                    pluginInstance.Plugin.NativeInstrumentsResource.ShortNamesState.ShouldSave = true;
                }

                foreach (var img in imagesToSave)
                {
                    if (img.State.State == NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated)
                    {
                        img.State.ShouldSave = true;
                    }
                }

                pluginInstance.Plugin.NativeInstrumentsResource.Save(pluginInstance.Plugin);
            }
        }

        public bool ShouldCreateScreenshot(IRemotePluginInstance pluginInstance)
        {
            var shouldCreateScreenshot = false;

            var niResource = pluginInstance.Plugin.NativeInstrumentsResource;

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
        public void GenerateResources(IRemotePluginInstance pluginInstance, bool force = false)
        {
            Image bmp;
            var niResource = pluginInstance.Plugin.NativeInstrumentsResource;

            var pluginName = pluginInstance.Plugin.PluginName;

            if (pluginInstance.Plugin.OverridePluginName)
            {
                pluginName = pluginInstance.Plugin.OverriddenPluginName;
            }

            if (niResource.ShortNamesState.State ==
                NativeInstrumentsResource.ResourceStates.Empty || force)
            {
                niResource.ShortNames.VB_shortname = pluginName;
                niResource.ShortNames.MKII_shortname = pluginName;
                niResource.ShortNames.MIKRO_shortname = pluginName;
                niResource.ShortNames.MST_shortname = pluginName;
                niResource.ShortNamesState.State = NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated;
            }

            if (niResource.CategoriesState.State == NativeInstrumentsResource.ResourceStates.Empty || force)
            {
                niResource.Categories.CategoryNames.Clear();
                var categoryDb = new CategoryDB
                {
                    FileType = pluginInstance.Plugin.PluginType == Plugin.PluginTypes.Instrument ? "INST" : "FX"
                };

                niResource.Categories.CategoryDB.Add(categoryDb);
                niResource.Categories.Vendor = pluginInstance.Plugin.PluginVendor;
                niResource.Categories.Product = pluginName;
                niResource.CategoriesState.State = NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated;
            }

            var pluginVendor = pluginInstance.Plugin.PluginVendor;

            niResource.MST_logo.ReplaceFromStream(
                RenderBigLogo(pluginName, pluginVendor, niResource.MST_logo.TargetSize, 35, 12),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);

            niResource.VB_logo.ReplaceFromStream(
                RenderLogo(pluginName, niResource.VB_logo.TargetSize, 30, new Point(7, 2)),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);

            niResource.OSO_logo.ReplaceFromStream(
                RenderLogo(pluginName, niResource.OSO_logo.TargetSize, 43, new Point(7, 2)),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);


            niResource.Color.SetRandomColor();
            niResource.ColorState.State = NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated;

            if (!ShouldCreateScreenshot(pluginInstance) && !force)
            {
                return;
            }

            if (!pluginInstance.IsEditorOpen)
            {
                // todo: create dummy image and generate random color
                pluginInstance.Plugin.Logger.Error(
                    "Tried to create a screenshot, but the editor was not open (maybe the plugin denied to open the editor)");
                return;
            }

            var data = pluginInstance.CreateScreenshot();

            if (data != null)
            {
                var ms = new MemoryStream();
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                bmp = Image.FromStream(ms);
            }
            else
            {
                // todo: create dummy image and generate random color
                pluginInstance.Plugin.Logger.Error("Failed to acquire screenshot.");
                return;
            }

            niResource.VB_artwork.ReplaceFromStream(GetScaledBitmap(bmp, niResource.VB_artwork.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);
            niResource.MST_plugin.ReplaceFromStream(GetScaledBitmap(bmp, niResource.MST_plugin.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);
            niResource.MST_artwork.ReplaceFromStream(GetScaledBitmap(bmp, niResource.MST_artwork.TargetSize),
                NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated);


            niResource.Color.BackgroundColor = DetectBestColor((Bitmap) bmp);
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

                if (col.GetBrightness() > 0.5)
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

            var resultColor = NiResourceColor.GetRandomColor();

            if (bestColor == null)
            {
                return resultColor;
            }

            var colorCode = bestColor.ColorCode;

            var convertedColor = ColorConverter.ConvertFromString(colorCode);
            if (convertedColor != null)
            {
                resultColor = (Color) convertedColor;
            }

            return resultColor;
        }

        private class PluginColor
        {
            public double Weight { get; set; }
            public string ColorCode { get; set; } = "";
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