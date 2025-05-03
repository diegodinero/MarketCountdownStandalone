using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MarketCountdownApp
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            return status == "OPEN"
                ? new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF))   // blue
                : new SolidColorBrush(Color.FromRgb(0xEB, 0x57, 0x57));  // red
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}