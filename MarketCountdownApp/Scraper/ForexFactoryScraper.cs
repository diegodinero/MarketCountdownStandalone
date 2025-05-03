using System;
using System.Collections.Generic;

namespace MarketCountdownApp.Scraper
{
    public static class ForexFactoryScraper
    {
        public static IEnumerable<MarketCountdownApp.EventItem> GetUpcoming(int count)
        {
            var list = new List<MarketCountdownApp.EventItem>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new MarketCountdownApp.EventItem
                {
                    Time = DateTime.UtcNow.AddMinutes(i * 30).ToString("HH:mm"),
                    Currency = "USD",
                    Description = $"Event {i+1}"
                });
            }
            return list;
        }
    }
}
