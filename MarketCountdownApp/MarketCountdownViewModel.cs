using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace MarketCountdownApp
{
    public class MarketCountdownViewModel : INotifyPropertyChanged
    {
        public string LondonOffset => GetOffset("London");
        public string NewYorkOffset => GetOffset("New York");
        public string SydneyOffset => GetOffset("Sydney");
        public string TokyoOffset => GetOffset("Tokyo");

        public ObservableCollection<EventItem> UpcomingEvents { get; } = new ObservableCollection<EventItem>();

        private readonly DispatcherTimer _timer;

        public MarketCountdownViewModel()
        {
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, (s, e) => Refresh(), Dispatcher.CurrentDispatcher);
            _timer.Start();
        }

        private void Refresh()
        {
            OnPropertyChanged(nameof(LondonOffset));
            OnPropertyChanged(nameof(NewYorkOffset));
            OnPropertyChanged(nameof(SydneyOffset));
            OnPropertyChanged(nameof(TokyoOffset));

            var events = Scraper.ForexFactoryScraper.GetUpcoming(5);
            UpcomingEvents.Clear();
            foreach (var ev in events) UpcomingEvents.Add(ev);
        }

        private string GetOffset(string city)
        {
            var nowUtc = DateTime.UtcNow;
            var nextOpen = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 8, 0, 0, DateTimeKind.Utc);
            if (nowUtc >= nextOpen) nextOpen = nextOpen.AddDays(1);
            var span = nextOpen - nowUtc;
            return $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class EventItem
    {
        public string Time { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}
