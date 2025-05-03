using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace MarketCountdownApp
{
    public class MarketCountdownViewModel : INotifyPropertyChanged
    {
        // Offsets
        public string LondonOffset => GetOffset("London");
        public string NewYorkOffset => GetOffset("New York");
        public string SydneyOffset => GetOffset("Sydney");
        public string TokyoOffset => GetOffset("Tokyo");

        // Local times
        public string LondonLocalTime { get; private set; }
        public string NewYorkLocalTime { get; private set; }
        public string SydneyLocalTime { get; private set; }
        public string TokyoLocalTime { get; private set; }

        // New “OPEN” / “CLOSED” status properties
        public string LondonStatus => IsOpen("London") ? "OPEN" : "CLOSED";
        public string NewYorkStatus => IsOpen("New York") ? "OPEN" : "CLOSED";
        public string SydneyStatus => IsOpen("Sydney") ? "OPEN" : "CLOSED";
        public string TokyoStatus => IsOpen("Tokyo") ? "OPEN" : "CLOSED";

        public ObservableCollection<EventItem> UpcomingEvents { get; }
            = new ObservableCollection<EventItem>();

        private readonly DispatcherTimer _timer;

        public MarketCountdownViewModel()
        {
            _timer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.Normal,
                (s, e) => Refresh(),
                Dispatcher.CurrentDispatcher);
            _timer.Start();
        }

        private void Refresh()
        {
            // Update offsets
            OnPropertyChanged(nameof(LondonOffset));
            OnPropertyChanged(nameof(NewYorkOffset));
            OnPropertyChanged(nameof(SydneyOffset));
            OnPropertyChanged(nameof(TokyoOffset));

            // Update local times
            LondonLocalTime = ConvertTime("GMT Standard Time");
            NewYorkLocalTime = ConvertTime("Eastern Standard Time");
            SydneyLocalTime = ConvertTime("AUS Eastern Standard Time");
            TokyoLocalTime = ConvertTime("Tokyo Standard Time");
            OnPropertyChanged(nameof(LondonLocalTime));
            OnPropertyChanged(nameof(NewYorkLocalTime));
            OnPropertyChanged(nameof(SydneyLocalTime));
            OnPropertyChanged(nameof(TokyoLocalTime));

            OnPropertyChanged(nameof(LondonStatus));
            OnPropertyChanged(nameof(NewYorkStatus));
            OnPropertyChanged(nameof(SydneyStatus));
            OnPropertyChanged(nameof(TokyoStatus));

            // Refresh “up next” list
            UpcomingEvents.Clear();
            foreach (var ev in Scraper.ForexFactoryScraper.GetUpcoming(5))
                UpcomingEvents.Add(ev);
        }

        private string ConvertTime(string windowsTimeZoneId)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
                var dt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
                return dt.ToString("HH:mm");
            }
            catch
            {
                return "--:--";
            }
        }

        private string GetOffset(string market)
        {
            var nowUtc = DateTime.UtcNow;
            // placeholder: next open at 08:00 UTC
            var next = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 8, 0, 0, DateTimeKind.Utc);
            if (nowUtc >= next) next = next.AddDays(1);
            var span = next - nowUtc;
            return $"{(int)span.TotalHours:00}:{span.Minutes:00}";
        }

        private bool IsOpen(string market)
        {
            // TODO: your real market-hours logic
            // Example: open daily 08:00–22:00 local
            var nowUtc = DateTime.UtcNow;
            TimeZoneInfo tz;
            switch (market)
            {
                case "London":
                    tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;
                case "New York":
                    tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    break;
                case "Sydney":
                    tz = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                    break;
                case "Tokyo":
                    tz = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    break;
                default:
                    tz = TimeZoneInfo.Utc;
                    break;
            }
            var local = TimeZoneInfo.ConvertTime(nowUtc, tz).TimeOfDay;
            return local >= TimeSpan.FromHours(8) && local < TimeSpan.FromHours(22);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class EventItem
    {
        public string Time { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}
