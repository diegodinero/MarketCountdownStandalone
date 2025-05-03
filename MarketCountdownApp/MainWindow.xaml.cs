using System.Windows;

namespace MarketCountdownApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MarketCountdownViewModel();
        }
    }
}
