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
        public MarketCountdownViewModel MarketVM { get; } = new MarketCountdownViewModel();

        public ObservableCollection<ForexEvent> UpcomingEvents { get; }
            = new ObservableCollection<ForexEvent>();

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
        public ForexEvent? NextEvent =>UpcomingEvents.FirstOrDefault(e => e.Occurrence > DateTime.Now);

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
                },
                Dispatcher.CurrentDispatcher);
            _countdownTimer.Start();

            _ = FetchEventsAsync();
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
