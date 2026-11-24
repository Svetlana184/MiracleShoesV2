using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MiracleShoesDesktop.Services
{

    public class ProductImageConverter : IValueConverter
    {
        private const string ResourcesPrefix = "/Resources/";
        private const string Placeholder = ResourcesPrefix + "placeholder.png";

        private static readonly BitmapImage PlaceholderImage =
            LoadFromResources(Placeholder) ?? new BitmapImage();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                var img = LoadFromResources(Normalize(url));
                if (img != null)
                    return img;
            }
            return PlaceholderImage;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static string Normalize(string url)
        {
            var s = url.Replace('\\', '/').Trim();

            if (s.StartsWith(ResourcesPrefix, StringComparison.OrdinalIgnoreCase))
                return s;

            if (s.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
                return "/" + s;

            if (s.StartsWith("/"))
                return ResourcesPrefix + s.TrimStart('/');

            return ResourcesPrefix + s;
        }

        private static BitmapImage? LoadFromResources(string resourcePath)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,{resourcePath}", UriKind.Absolute);
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