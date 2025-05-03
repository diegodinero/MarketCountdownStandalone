using System.Windows;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.Generic;

namespace MarketCountdownApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void OpenSettings_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Visible;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            // rebuild allowed currencies
            var allowed = new List<string>();
            if (AUDCheck.IsChecked == true) allowed.Add("AUD");
            if (CADCheck.IsChecked == true) allowed.Add("CAD");
            if (CHFCheck.IsChecked == true) allowed.Add("CHF");
            if (CNYCheck.IsChecked == true) allowed.Add("CNY");
            if (EURCheck.IsChecked == true) allowed.Add("EUR");
            if (GBPCheck.IsChecked == true) allowed.Add("GBP");
            if (JPYCheck.IsChecked == true) allowed.Add("JPY");
            if (NZDCheck.IsChecked == true) allowed.Add("NZD");
            if (USDCheck.IsChecked == true) allowed.Add("USD");

            // rebuild allowed impacts
            var allowedImpacts = new List<string>();
            if (HighImpactCheck.IsChecked == true) allowedImpacts.Add("High");
            if (MediumImpactCheck.IsChecked == true) allowedImpacts.Add("Medium");
            if (LowImpactCheck.IsChecked == true) allowedImpacts.Add("Low");
            if (HolidayImpactCheck.IsChecked == true) allowedImpacts.Add("Holiday");

            // TODO: apply these lists to filter your UpcomingEvents
            SettingsOverlay.Visibility = Visibility.Collapsed;
            
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
