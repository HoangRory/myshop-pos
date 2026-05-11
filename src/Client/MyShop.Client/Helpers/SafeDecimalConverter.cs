using System;
using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class SafeDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return null;

            if (decimal.TryParse(str, out var result))
                return result;

            return Binding.DoNothing; // keep the current value when input is invalid
        }
    }
}
