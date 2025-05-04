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
            ApplyFilter();
        }

        private void OpenSettings_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Visible;
            
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            // build allowed currencies from checkboxes
            var allowedCurrencies = new List<string>();
            if (AUDCheck.IsChecked == true) allowedCurrencies.Add("AUD");
            if (CADCheck.IsChecked == true) allowedCurrencies.Add("CAD");
            if (CHFCheck.IsChecked == true) allowedCurrencies.Add("CHF");
            if (CNYCheck.IsChecked == true) allowedCurrencies.Add("CNY");
            if (EURCheck.IsChecked == true) allowedCurrencies.Add("EUR");
            if (GBPCheck.IsChecked == true) allowedCurrencies.Add("GBP");
            if (JPYCheck.IsChecked == true) allowedCurrencies.Add("JPY");
            if (NZDCheck.IsChecked == true) allowedCurrencies.Add("NZD");
            if (USDCheck.IsChecked == true) allowedCurrencies.Add("USD");

            // build allowed impacts from checkboxes
            var allowedImpacts = new List<string>();
            if (HighImpactCheck.IsChecked == true) allowedImpacts.Add("High");
            if (MediumImpactCheck.IsChecked == true) allowedImpacts.Add("Medium");
            if (LowImpactCheck.IsChecked == true) allowedImpacts.Add("Low");
            if (HolidayImpactCheck.IsChecked == true) allowedImpacts.Add("Holiday");

            // get view and apply filter
            var view = CollectionViewSource.GetDefaultView(EventsListView.ItemsSource);
            if (view != null)
            {
                view.Filter = obj =>
                {
                    if (obj is ForexEvent ev)
                        return allowedCurrencies.Contains(ev.Currency)
                               && allowedImpacts.Contains(ev.Impact);
                    return false;
                };
                view.Refresh();
            }
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
