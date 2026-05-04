using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class DetailPanelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEditing = value != null;
            string side = parameter as string;

            if (side == "Left")
            {
                // Left: take all (*) if not editing, else fill remaining
                return new GridLength(1, GridUnitType.Star);
            }
            else if (side == "Right")
            {
                // Right: 0 when not editing, else fixed width
                return isEditing ? new GridLength(420) : new GridLength(0);
            }
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
