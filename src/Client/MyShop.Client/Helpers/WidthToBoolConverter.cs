using System;
using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    // Returns true when value (ActualWidth) is less than the provided ConverterParameter (double)
    public class WidthToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (!double.TryParse(value.ToString(), out double width)) return false;

            double threshold = 800d;
            if (parameter != null)
            {
                if (double.TryParse(parameter.ToString(), out double p)) threshold = p;
            }

            return width < threshold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
