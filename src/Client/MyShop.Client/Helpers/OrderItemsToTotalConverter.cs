using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using MyShop.Client.Models;

namespace MyShop.Client.Helpers
{
    public class OrderItemsToTotalConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<OrderItem> items)
            {
                decimal total = items.Sum(i => i.Total);
                return total;
            }
            return 0m;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
