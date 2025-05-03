using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;

namespace MarketCountdownApp
{
    public class MainWindowViewModel
    {
        // expose your existing VM for the top tiles:
        public MarketCountdownViewModel MarketVM { get; } = new MarketCountdownViewModel();

        // collection bound to the "Up Next" ListView:
        public ObservableCollection<ForexEvent> UpcomingEvents { get; }
            = new ObservableCollection<ForexEvent>();

        // bound to the bottom date pill:
        public string TodayDate => DateTime.Now.ToString("d MMM");

        private const string XmlFeedUrl = "https://nfs.faireconomy.media/ff_calendar_thisweek.xml";
        private readonly DispatcherTimer _timer;

        public MainWindowViewModel()
        {
            // re‑fetch every 5 minutes:
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _timer.Tick += async (_, __) => await FetchEventsAsync();
            _timer.Start();

            // initial load:
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
    }

    // reuse or move your ForexEvent class here:
    public class ForexEvent
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Title { get; set; } = "";
        public string Impact { get; set; } = "";
        public string? Forecast { get; set; }
        public string? Previous { get; set; }
    }
}
