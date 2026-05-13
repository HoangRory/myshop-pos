using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class DetailPanelWidthConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertCore(value, null, parameter as string);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object? editingValue = values.Length > 0 ? values[0] : null;
            double? availableWidth = null;

            if (values.Length > 1 && values[1] is double width)
            {
                availableWidth = width;
            }

            return ConvertCore(editingValue, availableWidth, parameter as string);
        }

        private static object ConvertCore(object? editingValue, double? availableWidth, string? parameter)
        {
            bool isEditing = editingValue != null;
            var (side, threshold) = ParseParameter(parameter);
            bool isCompact = availableWidth.HasValue && availableWidth.Value > 0 && availableWidth.Value < threshold;

            if (side == "Left")
            {
                if (isEditing && isCompact)
                {
                    return new GridLength(0);
                }

                return new GridLength(1, GridUnitType.Star);
            }

            if (side == "Right")
            {
                if (!isEditing)
                {
                    return new GridLength(0);
                }

                return isCompact ? new GridLength(1, GridUnitType.Star) : new GridLength(420);
            }

            return new GridLength(0);
        }

        private static (string Side, double Threshold) ParseParameter(string? parameter)
        {
            const double defaultThreshold = 980d;

            if (string.IsNullOrWhiteSpace(parameter))
            {
                return (string.Empty, defaultThreshold);
            }

            var parts = parameter.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var side = parts.Length > 0 ? parts[0] : string.Empty;

            if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold))
            {
                return (side, threshold);
            }

            return (side, defaultThreshold);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
