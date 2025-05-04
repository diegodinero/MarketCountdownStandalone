using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace MarketCountdownApp
{
    public class MarketCountdownViewModel : INotifyPropertyChanged
    {
        private class MarketInfo
        {
            public string Name { get; }
            public string TimeZoneId { get; }
            public TimeSpan Open1 { get; }    // first open
            public TimeSpan Close1 { get; }    // first close
            public TimeSpan? Open2 { get; }    // second open (Tokyo lunch)
            public TimeSpan? Close2 { get; }    // second close

            public MarketInfo(
                string name,
                string tz,
                TimeSpan open1, TimeSpan close1,
                TimeSpan? open2 = null, TimeSpan? close2 = null)
            {
                Name = name;
                TimeZoneId = tz;
                Open1 = open1;
                Close1 = close1;
                Open2 = open2;
                Close2 = close2;
            }
        }

        // define each exchange
        private readonly MarketInfo[] _markets = new[]
        {
            new MarketInfo("London",   "GMT Standard Time",
                           TimeSpan.FromHours(8), TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(30))),
            new MarketInfo("New York","Eastern Standard Time",
                           TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)), TimeSpan.FromHours(16)),
            new MarketInfo("Sydney",  "E. Australia Standard Time",
                           TimeSpan.FromHours(10), TimeSpan.FromHours(16)),
            new MarketInfo("Tokyo",   "Tokyo Standard Time",
                           TimeSpan.FromHours(9), TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30)),
                           TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(30)), TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(25)))
        };

        // Local times
        public string LondonLocalTime => ToLocal(_markets[0]).ToString("HH:mm");
        public string NewYorkLocalTime => ToLocal(_markets[1]).ToString("HH:mm");
        public string SydneyLocalTime => ToLocal(_markets[2]).ToString("HH:mm");
        public string TokyoLocalTime => ToLocal(_markets[3]).ToString("HH:mm");

        // OPEN/CLOSED
        public string LondonStatus => IsOpen(_markets[0]) ? "OPEN" : "CLOSED";
        public string NewYorkStatus => IsOpen(_markets[1]) ? "OPEN" : "CLOSED";
        public string SydneyStatus => IsOpen(_markets[2]) ? "OPEN" : "CLOSED";
        public string TokyoStatus => IsOpen(_markets[3]) ? "OPEN" : "CLOSED";

        // + / - display
        public string LondonDisplay => GetDisplay(_markets[0]);
        public string NewYorkDisplay => GetDisplay(_markets[1]);
        public string SydneyDisplay => GetDisplay(_markets[2]);
        public string TokyoDisplay => GetDisplay(_markets[3]);

        // progress bar
        public double LondonProgress => ComputeProgress(_markets[0]);
        public double NewYorkProgress => ComputeProgress(_markets[1]);
        public double SydneyProgress => ComputeProgress(_markets[2]);
        public double TokyoProgress => ComputeProgress(_markets[3]);

        // offset only
        public string LondonOffset => GetOffset(_markets[0]);
        public string NewYorkOffset => GetOffset(_markets[1]);
        public string SydneyOffset => GetOffset(_markets[2]);
        public string TokyoOffset => GetOffset(_markets[3]);

        public string TodayDate => ToLocal(_markets[1]).ToString("d MMM");

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
            foreach (var m in _markets)
            {
                var key = m.Name.Replace(" ", "");        // remove spaces
                OnPropertyChanged(key + "LocalTime");
                OnPropertyChanged(key + "Display");
                OnPropertyChanged(key + "Status");
                OnPropertyChanged(key + "Progress");
                OnPropertyChanged(key + "Offset");
            }
            OnPropertyChanged(nameof(TodayDate));

            UpcomingEvents.Clear();
            foreach (var ev in Scraper.ForexFactoryScraper.GetUpcoming(5))
                UpcomingEvents.Add(ev);
        }

        private DateTime ToLocal(MarketInfo m)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(m.TimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        private bool IsOpen(MarketInfo m)
        {
            var local = ToLocal(m);
            var t = local.TimeOfDay;
            var d = local.DayOfWeek;
            if (d == DayOfWeek.Saturday || d == DayOfWeek.Sunday)
                return false;

            // first session
            if (t >= m.Open1 && t < m.Close1)
                return true;
            // second session (if any)
            if (m.Open2.HasValue && m.Close2.HasValue &&
               t >= m.Open2.Value && t < m.Close2.Value)
                return true;

            return false;
        }

        private double ComputeProgress(MarketInfo m)
        {
            var local = ToLocal(m);
            var t = local.TimeOfDay;
            if (!IsOpen(m))
                return 0.0;

            TimeSpan start, length;
            if (m.Name == "Tokyo")
            {
                if (t < m.Close1)
                {
                    start = m.Open1;
                    length = m.Close1 - m.Open1;
                }
                else
                {
                    start = m.Open2.Value;
                    length = m.Close2.Value - m.Open2.Value;
                }
            }
            else
            {
                start = m.Open1;
                length = m.Close1 - m.Open1;
            }

            return (t - start).TotalMinutes / length.TotalMinutes;
        }

        private string GetDisplay(MarketInfo m)
        {
            // ?? SPECIAL?CASE LONDON TO USE 08:00?UTC ????????????????????????????????
            if (m.Name == "London")
            {
                DateTime nowUtc = DateTime.UtcNow;
                DateTime todayOpen = nowUtc.Date.AddHours(7);            // 08:00?UTC today
                DateTime nextOpenUtc = nowUtc < todayOpen
                    ? todayOpen
                    : todayOpen.AddDays(1);         // or tomorrow 08:00?UTC

                TimeSpan span = nextOpenUtc - nowUtc;
                return $"-{(int)span.TotalHours:00}:{span.Minutes:00}";
            }

            // ??????? SYDNEY: UTC?only countdown to 22:00 UTC ?????????
            if (m.Name == "Sydney")
            {
                DateTime nowUtc = DateTime.UtcNow;
                DateTime todayOpen = nowUtc.Date.AddHours(22);          // 22:00 UTC
                DateTime nextOpenUtc = nowUtc.Date.AddDays(nowUtc.TimeOfDay < TimeSpan.Zero ? 0 : 1);
                TimeSpan span = nextOpenUtc - nowUtc;
                return $"-{(int)span.TotalHours:00}:{span.Minutes:00}";
            }

            var local = ToLocal(m);
            var t = local.TimeOfDay;
            bool open = IsOpen(m);

            if (open)
            {
                // time since current session start
                TimeSpan since;
                if (m.Name == "Tokyo" && t >= m.Close1 && t < m.Open2)
                    since = TimeSpan.Zero;
                else if (t < m.Close1)
                    since = t - m.Open1;
                else
                    since = t - m.Open2.Value;

                return $"+{(int)since.TotalHours:00}:{since.Minutes:00}";
            }
            else
            {
                // compute next open local
                DateTime nextDate;
                var d = local.DayOfWeek;
                if (d == DayOfWeek.Saturday) nextDate = local.Date.AddDays(2);
                else if (d == DayOfWeek.Sunday) nextDate = local.Date.AddDays(1);
                else nextDate = local.Date;

                TimeSpan nextOpen = m.Open1;
                // if Tokyo lunch break, and we're between sessions:
                if (m.Name == "Tokyo" && t >= m.Close1 && t < m.Open2)
                    nextOpen = m.Open2.Value;
                else if (t >= m.Close1 && m.Name != "Tokyo")
                    nextDate = nextDate.AddDays(1);

                DateTime next = nextDate.Date.Add(nextOpen);
                if (m.Name != "Tokyo" && d != DayOfWeek.Saturday && d != DayOfWeek.Sunday && t >= m.Close1)
                    next = local.Date.AddDays(d == DayOfWeek.Friday ? 3 : 1).Add(nextOpen);

                var span = next - local;
                return $"-{(int)span.TotalHours:00}:{span.Minutes:00}";
            }
        }

        private string GetOffset(MarketInfo m)
            => GetDisplay(m).Substring(1);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class EventItem
    {
        public string Time { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}
