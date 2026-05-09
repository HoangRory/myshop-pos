using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? int.Parse(parameter.ToString()) : Binding.DoNothing;
        }
    }
}
