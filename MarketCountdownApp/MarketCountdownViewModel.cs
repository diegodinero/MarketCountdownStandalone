using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace MarketCountdownApp
{
    public class MarketCountdownViewModel : INotifyPropertyChanged
    {
        public string LondonDisplay => GetDisplay("London",
            "GMT Standard Time",      // Windows TZ ID
            new TimeSpan(8, 0, 0),     // opens 08:00 local
            new TimeSpan(16, 0, 0));   // closes 16:00 local

        public string NewYorkDisplay => GetDisplay("New York",
            "Eastern Standard Time",
            new TimeSpan(8, 0, 0),
            new TimeSpan(16, 0, 0));

        public string SydneyDisplay => GetDisplay("Sydney",
            "AUS Eastern Standard Time",
            new TimeSpan(8, 0, 0),
            new TimeSpan(16, 0, 0));

        public string TokyoDisplay => GetDisplay("Tokyo",
            "Tokyo Standard Time",
            new TimeSpan(8, 0, 0),
            new TimeSpan(16, 0, 0));

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

        // Percent of the open window that’s elapsed (0.0 = just opened, 1.0 = about to close)
        public double LondonProgress => ComputeProgress("London");
        public double NewYorkProgress => ComputeProgress("New York");
        public double SydneyProgress => ComputeProgress("Sydney");
        public double TokyoProgress => ComputeProgress("Tokyo");
        public string TodayDate => DateTime.Now.ToString("d MMM");

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
            OnPropertyChanged(nameof(LondonDisplay));
            OnPropertyChanged(nameof(NewYorkDisplay));
            OnPropertyChanged(nameof(SydneyDisplay));
            OnPropertyChanged(nameof(TokyoDisplay));

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

            OnPropertyChanged(nameof(LondonProgress));
            OnPropertyChanged(nameof(NewYorkProgress));
            OnPropertyChanged(nameof(SydneyProgress));
            OnPropertyChanged(nameof(TokyoProgress));

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

        private double ComputeProgress(string market)
        {
            // 1) Pick the right time zone for this market
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

            // 2) Convert now to that local time
            DateTime nowUtc = DateTime.UtcNow;
            TimeSpan localTime = TimeZoneInfo.ConvertTime(nowUtc, tz).TimeOfDay;

            // 3) Define your market open/close window
            TimeSpan openTime = TimeSpan.FromHours(8);   // e.g. 08:00
            TimeSpan closeTime = TimeSpan.FromHours(16);  // e.g. 16:00

            // 4) Compute a 0.0–1.0 ratio
            if (localTime < openTime) return 0.0;  // not opened yet
            if (localTime >= closeTime) return 1.0;  // already closed
            return (localTime - openTime).TotalMinutes
                   / (closeTime - openTime).TotalMinutes;
        }

        private string GetDisplay(string market, string tzId, TimeSpan open, TimeSpan close)
        {
            var nowUtc = DateTime.UtcNow;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var local = TimeZoneInfo.ConvertTime(nowUtc, tz).TimeOfDay;

            bool isOpenToday = local >= open && local < close;
            TimeSpan span;
            string sign;
            if (isOpenToday)
            {
                // time since open
                span = local - open;
                sign = "+";
            }
            else
            {
                // time until next open
                TimeSpan nextOpen = local < open
                    ? open
                    : open.Add(TimeSpan.FromDays(1));
                span = nextOpen - local;
                sign = "-";
            }
            return $"{sign}{(int)span.TotalHours:00}:{span.Minutes:00}";
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
