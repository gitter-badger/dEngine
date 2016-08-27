// IconProvider.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using dEditor.Utility;
using dEngine;
using dEngine.Services;

namespace dEditor.Framework.Services
{
    public static class IconProvider
    {
        private static readonly Dictionary<string, string> _icons;

        private static readonly Dictionary<string, Uri> _colouredIcons = new Dictionary<string, Uri>();

        static IconProvider()
        {
            _icons =
                HttpService.JsonDecode<Dictionary<string, string>>(
                    ContentProvider.DownloadString("editor://IconDictionary.json"));
        }

        public static string GetIconName(Type type)
        {
            string iconName;

            if (!_icons.TryGetValue(type.Name, out iconName))
                iconName = type.BaseType == null ? "default" : GetIconName(type.BaseType);

            return iconName;
        }

        public static Uri GetIconUri(Type type)
        {
            var name = type != null ? GetIconName(type) : "default";
            return new Uri($"/dEditor;component/Content/Icons/Objects/{name}.png", UriKind.Relative);
        }

        public static Uri ModifyIconHue(Uri source, int hue)
        {
            if (hue == 0)
                return source;

            var name = $"Icon_{source.GetHashCode()}_{hue}.png";

            Uri existing;
            if (_colouredIcons.TryGetValue(name, out existing))
                return existing;

            var x = Application.GetResourceStream(source);
            var dec = BitmapDecoder.Create(x.Stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var image = dec.Frames[0];
            var pixels = new byte[image.PixelWidth*image.PixelHeight*4];
            image.CopyPixels(pixels, image.PixelWidth*4, 0);

            for (var i = 0; i < pixels.Length; i += 4)
            {
                var blue = pixels[i];
                var green = pixels[i + 1];
                var red = pixels[i + 2];

                double h;
                double s;
                double l;
                ColorHelper.RGB2HSL(red, green, blue, out h, out s, out l);
                h = hue/360.0;
                ColorHelper.HSL2RGB(h, s, l, out red, out green, out blue);

                pixels[i] = blue;
                pixels[i + 1] = green;
                pixels[i + 2] = red;
            }

            var bmp = new WriteableBitmap(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY,
                PixelFormats.Pbgra32, null);
            bmp.WritePixels(new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight), pixels, image.PixelWidth*4, 0);


            var path = Path.Combine(Engine.TempPath, name);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
            }

            var uri = new Uri(path, UriKind.Absolute);
            _colouredIcons[name] = uri;
            return uri;
        }
    }
}