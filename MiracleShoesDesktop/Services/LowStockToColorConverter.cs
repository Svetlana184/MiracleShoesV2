using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace MiracleShoesDesktop.Services
{
    public class LowStockToColorConverter : IValueConverter
    {
        private const int LowStockThreshold = 3;
        private static readonly SolidColorBrush LowStockBrush =
            new((Color)ColorConverter.ConvertFromString("#FF8080"));

        static LowStockToColorConverter()
        {
            LowStockBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int amount && amount <= LowStockThreshold)
                return LowStockBrush;
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
