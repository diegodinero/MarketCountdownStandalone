using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MarketCountdownApp
{
    public partial class CircularProgressBar_UC : UserControl
    {
        //–– DependencyProperties
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(CircularProgressBar_UC),
                new PropertyMetadata(0.0, OnProgressPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(CircularProgressBar_UC),
                new PropertyMetadata(1.0, OnProgressPropertyChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularProgressBar_UC),
                new PropertyMetadata(0.0, OnProgressPropertyChanged));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(CircularProgressBar_UC),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 193, 7))));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(CircularProgressBar_UC),
                new PropertyMetadata(6.0, OnProgressPropertyChanged));

        //–– CLR wrappers
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public new Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public CircularProgressBar_UC()
        {
            InitializeComponent();
            SizeChanged += (s, e) => UpdateArc();
        }

        private static void OnProgressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CircularProgressBar_UC)d).UpdateArc();
        }

        private void UpdateArc()
        {
            // normalize 0–1
            double range = Maximum - Minimum;
            double pct = range > 0 ? (Value - Minimum) / range : 0;
            pct = Math.Max(0, Math.Min(1, pct));

            double w = ActualWidth;
            double h = ActualHeight;
            double radius = Math.Min(w, h) / 2;
            double cx = w / 2, cy = h / 2;

            // convert pct → angle (start at 12 o’clock)
            double angle = 360 * pct;
            double theta = (Math.PI / 180) * (angle - 90);

            // end point on circle
            double x = cx + radius * Math.Cos(theta);
            double y = cy + radius * Math.Sin(theta);

            bool isLarge = angle > 180;

            var fig = new PathFigure { StartPoint = new Point(cx, cy - radius) };
            var arc = new ArcSegment
            {
                Point = new Point(x, y),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLarge
            };
            fig.Segments.Clear();
            fig.Segments.Add(arc);

            var geo = new PathGeometry();
            geo.Figures.Add(fig);

            PART_Path.Data = geo;
        }
    }
}
