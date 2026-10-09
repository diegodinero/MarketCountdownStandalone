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
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            AUDCheck.IsChecked = Properties.Settings.Default.ShowAUD;
            CADCheck.IsChecked = Properties.Settings.Default.ShowCAD;
            CHFCheck.IsChecked = Properties.Settings.Default.ShowCHF;
            CNYCheck.IsChecked = Properties.Settings.Default.ShowCNY;
            EURCheck.IsChecked = Properties.Settings.Default.ShowEUR;
            GBPCheck.IsChecked = Properties.Settings.Default.ShowGBP;
            JPYCheck.IsChecked = Properties.Settings.Default.ShowJPY;
            NZDCheck.IsChecked = Properties.Settings.Default.ShowNZD;
            USDCheck.IsChecked = Properties.Settings.Default.ShowUSD;
            ShowNextEventCheck.IsChecked = Properties.Settings.Default.ShowNextEventToggle;
            AnnouncerSoundsCheck.IsChecked = Properties.Settings.Default.AnnouncerSoundsEnabled;
            Use24HourCheck.IsChecked = Properties.Settings.Default.Use24Hour;
            //HighImpactCheck.IsChecked = Properties.Settings.Default.;
            //MediumImpactCheck.IsChecked = Properties.Settings.Default.;
            //LowImpactCheck.IsChecked = Properties.Settings.Default.;
            //HolidayImpactCheck.IsChecked = Properties.Settings.Default.;

            DarkModeCheck.IsChecked = Properties.Settings.Default.IsDarkMode;
            LaptopModeCheck.IsChecked = Properties.Settings.Default.IsLaptopMode;
            
            // Initialize ViewModel properties from settings
            viewModel.ShowAUD = Properties.Settings.Default.ShowAUD;
            viewModel.ShowCAD = Properties.Settings.Default.ShowCAD;
            viewModel.ShowCHF = Properties.Settings.Default.ShowCHF;
            viewModel.ShowCNY = Properties.Settings.Default.ShowCNY;
            viewModel.ShowEUR = Properties.Settings.Default.ShowEUR;
            viewModel.ShowGBP = Properties.Settings.Default.ShowGBP;
            viewModel.ShowJPY = Properties.Settings.Default.ShowJPY;
            viewModel.ShowNZD = Properties.Settings.Default.ShowNZD;
            viewModel.ShowUSD = Properties.Settings.Default.ShowUSD;
            viewModel.IsDarkMode = Properties.Settings.Default.IsDarkMode;
            viewModel.ShowNextEventToggle = Properties.Settings.Default.ShowNextEventToggle;
            viewModel.AnnouncerSoundsEnabled = Properties.Settings.Default.AnnouncerSoundsEnabled;
            viewModel.Use24Hour = Properties.Settings.Default.Use24Hour;
            
            SetExpanded(false);
            ApplyFilter();
            
            // Apply laptop mode if enabled
            if (Properties.Settings.Default.IsLaptopMode)
            {
                ApplyLaptopMode();
            }
        }

        private void OpenSettings_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Visible;
            
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
            ApplyFilter();
            Properties.Settings.Default.ShowAUD = AUDCheck.IsChecked == true;
            Properties.Settings.Default.ShowCAD = CADCheck.IsChecked == true;
            Properties.Settings.Default.ShowCHF = CHFCheck.IsChecked == true;
            Properties.Settings.Default.ShowCNY = CNYCheck.IsChecked == true;
            Properties.Settings.Default.ShowEUR = EURCheck.IsChecked == true;
            Properties.Settings.Default.ShowGBP = GBPCheck.IsChecked == true;
            Properties.Settings.Default.ShowJPY = JPYCheck.IsChecked == true;
            Properties.Settings.Default.ShowNZD = NZDCheck.IsChecked == true;
            Properties.Settings.Default.ShowUSD = USDCheck.IsChecked == true;
            
            Properties.Settings.Default.ShowNextEventToggle = ShowNextEventCheck.IsChecked == true;
            Properties.Settings.Default.IsDarkMode = DarkModeCheck.IsChecked == true;
            Properties.Settings.Default.AnnouncerSoundsEnabled = AnnouncerSoundsCheck.IsChecked == true;
            Properties.Settings.Default.Use24Hour = Use24HourCheck.IsChecked == true;
            Properties.Settings.Default.IsLaptopMode = LaptopModeCheck.IsChecked == true;

            // Apply changes to the ViewModel immediately so announcer/filtering respects new settings
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ShowAUD = AUDCheck.IsChecked == true;
                vm.ShowCAD = CADCheck.IsChecked == true;
                vm.ShowCHF = CHFCheck.IsChecked == true;
                vm.ShowCNY = CNYCheck.IsChecked == true;
                vm.ShowEUR = EURCheck.IsChecked == true;
                vm.ShowGBP = GBPCheck.IsChecked == true;
                vm.ShowJPY = JPYCheck.IsChecked == true;
                vm.ShowNZD = NZDCheck.IsChecked == true;
                vm.ShowUSD = USDCheck.IsChecked == true;

                vm.IsDarkMode = DarkModeCheck.IsChecked == true;
                vm.ShowNextEventToggle = ShowNextEventCheck.IsChecked == true;
                vm.AnnouncerSoundsEnabled = AnnouncerSoundsCheck.IsChecked == true;
                vm.Use24Hour = Use24HourCheck.IsChecked == true;

                // Clear any pending sounds for currencies that are now hidden
                vm.ClearPlayedSoundsForHiddenCurrencies();
            }
            
            // any others�
            
            // finally, persist to disk
            Properties.Settings.Default.Save();
            
            // Apply laptop mode changes
            if (LaptopModeCheck.IsChecked == true)
            {
                ApplyLaptopMode();
            }
            else
            {
                ApplyDesktopMode();
            }
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
            // get the CollectionView for the grouped items
            var view = CollectionViewSource.GetDefaultView(EventsListView.ItemsSource) as CollectionView;
            if (view?.Groups == null) return;

            // find the "today" group
            string todayName = DateTime.Now.DayOfWeek.ToString();
            var todayGroup = view.Groups
                                  .OfType<CollectionViewGroup>()
                                  .FirstOrDefault(g => g.Name.ToString() == todayName);
            if (todayGroup == null) return;

            // use ForexEvent.Occurrence for comparisons
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime utcNow = DateTime.UtcNow;
            DateTime estNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, estZone);

            // find the first item whose scheduled time is still in the future
            object nextItem = todayGroup.Items
                .Cast<ForexEvent>()
                .FirstOrDefault(ev => {
                    return ev.Occurrence >= estNow;
                });

            // if none remain, fall back to first item in the group
            if (nextItem == null && todayGroup.Items.Count > 0)
                nextItem = todayGroup.Items[0];

            if (nextItem != null)
                EventsListView.ScrollIntoView(nextItem);
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

        private void AnnouncerSoundsCheck_Checked(object sender, RoutedEventArgs e)
        {
            // Play the raidentorpedo.wav sound when the checkbox is checked
            if (DataContext is MainWindowViewModel vm)
            {
                vm.PlaySound("raidentorpedo.wav");
            }
        }

        private void AnnouncerSoundsCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            // Do nothing when unchecked (no sound played as per requirements)
        }
        
        private void ApplyLaptopMode()
        {
            // Apply compact layout for laptop mode
            this.Width = 550;
            this.Height = 480;
            
            // Reduce tile section height
            var tileRowDef = RootGrid.RowDefinitions[1];
            tileRowDef.Height = new GridLength(200);
            
            // Reduce DragBar padding and margins
            DragBar.Padding = new Thickness(2);
            DragBar.Margin = new Thickness(2);
            
            // Reduce icon sizes and margins
            foreach (var child in DragBar.Child is StackPanel sp ? sp.Children.OfType<Image>() : new List<Image>())
            {
                child.Width = 28;
                child.Height = 28;
                child.Margin = new Thickness(2);
            }
            
            // Reduce main content margins
            MainContent.Margin = new Thickness(6);
            
            // Reduce font sizes in tiles - this would require iterating through all tiles
            // For now, we'll just adjust the overall window size and let it reflow
        }
        
        private void ApplyDesktopMode()
        {
            // Apply full layout for desktop mode
            this.Width = 600;
            this.Height = 530;
            
            // Restore tile section height
            var tileRowDef = RootGrid.RowDefinitions[1];
            tileRowDef.Height = new GridLength(240);
            
            // Restore DragBar padding and margins
            DragBar.Padding = new Thickness(4);
            DragBar.Margin = new Thickness(4);
            
            // Restore icon sizes and margins
            foreach (var child in DragBar.Child is StackPanel sp ? sp.Children.OfType<Image>() : new List<Image>())
            {
                child.Width = 32;
                child.Height = 32;
                child.Margin = new Thickness(4);
            }
            
            // Restore main content margins
            MainContent.Margin = new Thickness(8);
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
