using System;
using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class SafeDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return 0m;

            if (decimal.TryParse(str, out var result))
                return result;

            return 0m; // fallback
        }
    }
}
