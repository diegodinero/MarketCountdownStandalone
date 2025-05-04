using System;
using System.Windows;

namespace MarketCountdownApp
{
    public partial class App : Application
    {
        private ResourceDictionary light;
        private ResourceDictionary dark;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // load both dictionaries once
            light = new ResourceDictionary { Source = new Uri("LightTheme.xaml", UriKind.Relative) };
            dark = new ResourceDictionary { Source = new Uri("DarkTheme.xaml", UriKind.Relative) };

            // ensure light is applied
            ApplyTheme(isDark: false);
        }

        public void ApplyTheme(bool isDark)
        {
            var md = Resources.MergedDictionaries;
            md.Clear();
            md.Add(isDark ? dark : light);
        }
    }
}