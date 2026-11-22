using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MiracleShoesDesktop.Services
{
    public class ProductImageConverter : IValueConverter
    {
        private const string Placeholder = "/Resources/placeholder.png";
        private static readonly BitmapImage PlaceholderImage = LoadOrNull(Placeholder);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                var img = TryLoad(url);
                if (img != null) return img;
            }
            return PlaceholderImage;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static BitmapImage? TryLoad(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
                {
                    if (absolute.IsFile && !File.Exists(absolute.LocalPath))
                        return null;
                    return Load(absolute);
                }

                var normalized = url.Replace('\\', '/');
                if (!normalized.StartsWith("/")) normalized = "/" + normalized;
                return Load(new Uri($"pack://application:,,,{normalized}", UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage? LoadOrNull(string url) => TryLoad(url);

        private static BitmapImage? Load(Uri uri)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
