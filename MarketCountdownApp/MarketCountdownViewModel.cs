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
