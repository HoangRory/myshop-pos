using System;
using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    /// <summary>
    /// Converts between bool (IsAscending) and string ("Tăng dần" / "Giảm dần")
    /// </summary>
    public class BoolToSortDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAscending)
            {
                return isAscending ? "Tăng dần" : "Giảm dần";
            }
            return "Tăng dần";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string direction)
            {
                return direction == "Tăng dần";
            }
            return true;
        }
    }
}
