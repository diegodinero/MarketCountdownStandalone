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
            var nowLocal = ToLocal(m);
            var t = nowLocal.TimeOfDay;
            DateTime today = nowLocal.Date;
            DateTime open1 = today.Add(m.Open1);
            DateTime close1 = today.Add(m.Close1);
            DateTime open2 = m.Open2.HasValue ? today.Add(m.Open2.Value) : DateTime.MinValue;
            DateTime close2 = m.Close2.HasValue ? today.Add(m.Close2.Value) : DateTime.MinValue;

            // Helper to clamp 0–1
            double Clamp01(double v) => Math.Max(0.0, Math.Min(1.0, v));

            // If market is open and not in Tokyo lunch gap
            if (IsOpen(m) && !(m.Name == "Tokyo" && t >= m.Close1 && t < m.Open2.Value))
            {
                DateTime sessionStart, sessionEnd;
                if (t < m.Close1 || !m.Open2.HasValue)
                {
                    sessionStart = open1;
                    sessionEnd = close1;
                }
                else
                {
                    sessionStart = open2;
                    sessionEnd = close2;
                }

                double totalMins = (sessionEnd - sessionStart).TotalMinutes;
                double elapsed = (nowLocal - sessionStart).TotalMinutes;
                return Clamp01(elapsed / totalMins);
            }

            // CLOSED or Tokyo lunch gap: march dot toward next open
            DateTime lastClose, nextOpen;
            if (nowLocal < open1)
            {
                lastClose = today.AddDays(-1).Add(m.Close1);
                nextOpen = open1;
            }
            else if (m.Open2.HasValue && t >= m.Close1 && t < m.Open2.Value)
            {
                // Tokyo lunch
                lastClose = close1;
                nextOpen = open2;
            }
            else
            {
                lastClose = m.Close2.HasValue ? close2 : close1;
                nextOpen = open1.AddDays((nowLocal < open1) ? 0 : 1);
                if (nowLocal.DayOfWeek == DayOfWeek.Saturday) nextOpen = open1.AddDays(2);
                if (nowLocal.DayOfWeek == DayOfWeek.Sunday) nextOpen = open1.AddDays(1);
            }

            double downtimeMins = (nextOpen - lastClose).TotalMinutes;
            double sinceClose = (nowLocal - lastClose).TotalMinutes;
            return Clamp01(sinceClose / downtimeMins);
        }



        private string GetDisplay(MarketInfo m)
        {
            var local = ToLocal(m);
            var t = local.TimeOfDay;
            bool open = IsOpen(m);
            // ?? SPECIAL?CASE LONDON TO USE 08:00?UTC ????????????????????????????????
            if (m.Name == "London")
            {
                // compute today’s 08:00 local London
                DateTime todayOpenLocal = local.Date.Add(m.Open1);
                if (open && t < m.Close1)    // market is open now
                {
                    TimeSpan sinceOpen = local - todayOpenLocal;
                    return $"+{(int)sinceOpen.TotalHours:00}:{sinceOpen.Minutes:00}";
                }
                else
                {
                    // next open is either today at 08:00 (if before open) or tomorrow
                    DateTime nextOpenLocal = local < todayOpenLocal
                        ? todayOpenLocal
                        : todayOpenLocal.AddDays(1);
                    TimeSpan untilOpen = nextOpenLocal - local;
                    return $"-{(int)untilOpen.TotalHours:00}:{untilOpen.Minutes:00}";
                }
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

                // ? INSERT THESE TWO LINES ???
                if (next <= local)
                    next = next.AddDays(1);
                // ??? INSERT THESE TWO LINES

                var span = next - local;

                // take absolute values so we only ever print one “?”
                var hours = Math.Abs((int)span.TotalHours);
                var mins = Math.Abs(span.Minutes);

                return $"-{hours:00}:{mins:00}";
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
