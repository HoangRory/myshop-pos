using System.Globalization;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class RevenueToHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] là doanh thu thực tế (decimal)
            // values[1] là mốc doanh thu tối đa (string/double)
            if (values != null && values.Length >= 2 && values[0] is decimal revenue)
            {
                double maxRevenue = System.Convert.ToDouble(values[1]);
                double actualRevenue = (double)revenue;

                // Giới hạn chiều cao tối đa của bar là 200px
                double height = (actualRevenue / maxRevenue) * 200;

                // Trả về tối thiểu 2px để vẫn thấy thanh nếu doanh thu nhỏ, và tối đa 200px
                return Math.Clamp(height, 2, 200);
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
