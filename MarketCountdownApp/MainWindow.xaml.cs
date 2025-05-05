using System.Windows;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace MarketCountdownApp
{
    public partial class MainWindow : Window
    {

        private bool isExpanded = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            SetExpanded(false);
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

        private void OnIconBarClick(object sender, MouseButtonEventArgs e)
            => SetExpanded(true);


        private void DragBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // if they clicked on anything _but_ an Image (your icons),
            // allow window?drag; otherwise let the icon clicks fire.
            if (!(e.OriginalSource is Image))
                this.DragMove();
        }

        private void SetExpanded(bool expand)
        {
            isExpanded = expand;
            MainContent.Visibility = expand ? Visibility.Visible : Visibility.Collapsed;
            CollapseIcon.Visibility = expand ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnExpandClick(object sender, MouseButtonEventArgs e)
        {
            // restore the Window background
            RootWindow.Background = (Brush)FindResource("WindowBackgroundBrush");

            // restore the panel background
            MainBorder.Background = (Brush)FindResource("PanelBackgroundBrush");

            MainContent.Visibility = Visibility.Visible;
            UpNextContent.Visibility = Visibility.Visible;

            ExpandIcon.Visibility = Visibility.Collapsed;
            CollapseIcon.Visibility = Visibility.Visible;

            SettingsGear.Visibility = Visibility.Visible;
            DatePillButton.Visibility = Visibility.Visible;
        }

        private void OnCollapseClick(object sender, MouseButtonEventArgs e)
        {
            // hide everything
            MainContent.Visibility = Visibility.Collapsed;
            UpNextContent.Visibility = Visibility.Collapsed;

            ExpandIcon.Visibility = Visibility.Visible;
            CollapseIcon.Visibility = Visibility.Collapsed;

            SettingsGear.Visibility = Visibility.Collapsed;
            DatePillButton.Visibility = Visibility.Collapsed;

            // make both your Window _and_ your MainBorder fully transparent
            RootWindow.Background = Brushes.Transparent;
            MainBorder.Background = Brushes.Transparent;
        }

        private void DatePillButton_Click(object sender, RoutedEventArgs e)
        {
            // pull the grouped CollectionView from your ListView
            var view = CollectionViewSource
                    .GetDefaultView(EventsListView.ItemsSource)
                as CollectionView;
            if (view?.Groups == null) return;

            var todayName = DateTime.Now.DayOfWeek.ToString();

            // needs System.Linq
            var todayGroup = view.Groups
                .OfType<CollectionViewGroup>()
                .FirstOrDefault(g => g.Name.ToString() == todayName);
            if (todayGroup?.ItemCount > 0)
                EventsListView.ScrollIntoView(todayGroup.Items[0]);
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
