using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MarketCountdownApp
{
    public class IconSourceConverter : IMultiValueConverter
    {
        // values[0] = Tag (string), values[1] = IsDarkMode (bool)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string tag && values.Length > 1 && values[1] is bool isDark)
            {
                var fileName = isDark ? $"{tag}_White.png" : $"{tag}.png";
                // pack URI for Resource build action
                var uri = new Uri($"pack://application:,,,/MarketCountdownApp;component/Icons/{fileName}", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}