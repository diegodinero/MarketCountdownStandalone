using System.Windows;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MarketCountdownApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }

    public class ImpactToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((value as string)?.ToLowerInvariant())
            {
                case "high": return Brushes.Red;
                case "medium": return Brushes.Orange;
                case "low": return Brushes.Green;
                case "holiday": return Brushes.Blue;
                default: return Brushes.LightGray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
