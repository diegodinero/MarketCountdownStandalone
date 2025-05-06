using System;
using System.Collections.ObjectModel;
using System.ComponentModel;        // ← add
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace MarketCountdownApp
{
    // ← implement INotifyPropertyChanged
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        private static readonly string[] ImpactOrder = { "High", "Medium", "Low", "Holiday" };
        public MarketCountdownViewModel MarketVM { get; } = new MarketCountdownViewModel();

        public ObservableCollection<ForexEvent> UpcomingEvents { get; }
            = new ObservableCollection<ForexEvent>();

        // THESE BACKING FIELDS:
        private bool _showUSD = true;
        private bool _showEUR = true;
        private bool _showGBP = true;
        private bool _showCAD = true;
        private bool _showCHF = true;
        private bool _showAUD = true;
        private bool _showCNY = true;
        private bool _showNZD = true;
        private bool _showJPY = true;

        // AND THESE PROPERTIES:
        public bool ShowUSD
        {
            get => _showUSD;
            set
            {
                if (_showUSD == value) return;
                _showUSD = value;
                OnPropertyChanged(nameof(ShowUSD));
                RefreshNextEvent();
            }
        }
        public bool ShowEUR
        {
            get => _showEUR;
            set
            {
                if (_showEUR == value) return;
                _showEUR = value;
                OnPropertyChanged(nameof(ShowEUR));
                RefreshNextEvent();
            }
        }
        public bool ShowGBP
        {
            get => _showGBP;
            set
            {
                if (_showGBP == value) return;
                _showGBP = value;
                OnPropertyChanged(nameof(ShowGBP));
                RefreshNextEvent();
            }
        }
        public bool ShowCAD
        {
            get => _showCAD;
            set
            {
                if (_showCAD == value) return;
                _showCAD = value;
                OnPropertyChanged(nameof(ShowCAD));
                RefreshNextEvent();
            }
        }
        public bool ShowCHF
        {
            get => _showCHF;
            set
            {
                if (_showCHF == value) return;
                _showCHF = value;
                OnPropertyChanged(nameof(ShowCHF));
                RefreshNextEvent();
            }
        }
        public bool ShowAUD
        {
            get => _showAUD;
            set
            {
                if (_showAUD == value) return;
                _showAUD = value;
                OnPropertyChanged(nameof(ShowAUD));
                RefreshNextEvent();
            }
        }
        public bool ShowCNY
        {
            get => _showCNY;
            set
            {
                if (_showCNY == value) return;
                _showCNY = value;
                OnPropertyChanged(nameof(ShowCNY));
                RefreshNextEvent();
            }
        }
        public bool ShowNZD
        {
            get => _showNZD;
            set
            {
                if (_showNZD == value) return;
                _showNZD = value;
                OnPropertyChanged(nameof(ShowNZD));
                RefreshNextEvent();
            }
        }
        public bool ShowJPY
        {
            get => _showJPY;
            set
            {
                if (_showJPY == value) return;
                _showJPY = value;
                OnPropertyChanged(nameof(ShowJPY));
                RefreshNextEvent();
            }
        }

        private bool IsCurrencyVisible(string c) => c switch
        {
            "USD" => ShowUSD,
            "EUR" => ShowEUR,
            "GBP" => ShowGBP,
            "CAD" => ShowCAD,
            "CHF" => ShowCHF,
            "AUD" => ShowAUD,
            "CNY" => ShowCNY,
            "NZD" => ShowNZD,
            "JPY" => ShowJPY,
            _ => true
        };

        private bool isDarkMode;
        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                if (isDarkMode == value) return;
                isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));    // ← raise notification

                if (Application.Current is App app)
                    app.ApplyTheme(isDarkMode);

            }
        }

        private bool _showNextEventToggle = true;
        public bool ShowNextEventToggle
        {
            get => _showNextEventToggle;
            set
            {
                if (_showNextEventToggle == value) return;
                _showNextEventToggle = value;
                OnPropertyChanged(nameof(ShowNextEventToggle));
            }
        }

        public string TodayDate => DateTime.Now.ToString("d MMM");

        private const string XmlFeedUrl = "https://nfs.faireconomy.media/ff_calendar_thisweek.xml";
        private readonly DispatcherTimer _timer;
        // 1) The very next event that has not yet passed:
        public ForexEvent? NextEvent
        {
            get
            {
                var now = DateTime.Now;
                var future = UpcomingEvents.Where(e => e.Occurrence >= now && IsCurrencyVisible(e.Currency)).ToList();

                if (!future.Any())
                    return null;

                var nextTime = future.Min(e => e.Occurrence);

                return future.Where(e => e.Occurrence == nextTime).OrderBy(e =>
                    {
                        var idx = Array.IndexOf(ImpactOrder, e.Impact);
                        return idx < 0 ? ImpactOrder.Length : idx;
                    })
                    .First();
            }
        }

        // 2) The text line that shows currency/title:
        public string NextEventText =>
            NextEvent != null
                ? $"{NextEvent.Currency} — {NextEvent.Title}"
                : "No upcoming event";

        // 3) The live countdown string:
        public string NextEventCountdown
        {
            get
            {
                if (NextEvent == null)
                    return "";

                // use the Occurrence we already stored on the event
                var dt = NextEvent.Occurrence;

                // here is the missing declaration
                var span = dt - DateTime.Now;

                if (span <= TimeSpan.Zero)
                    return "00:00:00";

                // now span.TotalHours, span.Minutes, span.Seconds are in scope
                return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
            }
        }


        private DispatcherTimer _countdownTimer;

        public MainWindowViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _timer.Tick += async (_, __) => await FetchEventsAsync();
            _timer.Start();
            _countdownTimer = new DispatcherTimer(TimeSpan.FromSeconds(1),
                DispatcherPriority.Normal,
                (_, __) =>
                {
                    OnPropertyChanged(nameof(NextEvent));
                    OnPropertyChanged(nameof(NextEventText));
                    OnPropertyChanged(nameof(NextEventCountdown));
                    OnPropertyChanged(nameof(TodayDate));
                },
                Dispatcher.CurrentDispatcher);
            _countdownTimer.Start();

            _ = FetchEventsAsync();
        }

        /// <summary>
        /// Re‑compute the “NextEvent” and its text/countdown by raising PropertyChanged
        /// </summary>
        private void RefreshNextEvent()
        {
            OnPropertyChanged(nameof(NextEvent));
            OnPropertyChanged(nameof(NextEventText));
            OnPropertyChanged(nameof(NextEventCountdown));
        }

        private async Task FetchEventsAsync()
        {
            try
            {
                using var http = new HttpClient();
                var xmlText = await http.GetStringAsync(XmlFeedUrl);
                var doc = XDocument.Parse(xmlText);

                var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                var all = doc.Descendants("event")
                             .Select(x =>
                             {
                                 var date = DateTime.ParseExact(
                                     x.Element("date")!.Value.Trim(),
                                     "MM-dd-yyyy",
                                     CultureInfo.InvariantCulture);

                                 var timePart = DateTime.ParseExact(
                                     x.Element("time")!.Value.Trim(),
                                     "h:mmtt",
                                     CultureInfo.InvariantCulture);

                                 var utcDt = DateTime.SpecifyKind(
                                     new DateTime(
                                         date.Year, date.Month, date.Day,
                                         timePart.Hour, timePart.Minute, 0),
                                     DateTimeKind.Utc);

                                 var estDt = TimeZoneInfo.ConvertTimeFromUtc(utcDt, estZone);

                                 return new ForexEvent
                                 {
                                     Date = date,
                                     Time = estDt.ToString("HH:mm"),
                                     Currency = x.Element("country")!.Value.Trim(),
                                     Title = x.Element("title")!.Value.Trim(),
                                     Impact = x.Element("impact")!.Value.Trim(),
                                     Forecast = x.Element("forecast")?.Value.Trim(),
                                     Previous = x.Element("previous")?.Value.Trim(),
                                     Occurrence = estDt,
                                 };
                             })
                             .OrderBy(e => e.Date)
                             .ThenBy(e => e.Time)
                             .ToList();

                UpcomingEvents.Clear();
                foreach (var ev in all)
                    UpcomingEvents.Add(ev);
            }
            catch
            {
                // swallow or surface errors as you like
            }
        }

        // ← INotifyPropertyChanged boilerplate ↓↓↓
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ForexEvent
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Title { get; set; } = "";
        public string Impact { get; set; } = "";
        public string? Forecast { get; set; }
        public string? Previous { get; set; }
        public DateTime Occurrence { get; set; }
        public bool IsPast => Occurrence < DateTime.Now;
    }
}
