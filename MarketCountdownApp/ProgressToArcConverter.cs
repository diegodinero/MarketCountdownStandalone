using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MarketCountdownApp
{
    /// <summary>
    /// Converts a double 0–1 progress value into a circular arc PathGeometry.
    /// </summary>
    public class ProgressToArcConverter : IValueConverter
    {
        // You can tweak these defaults or pass Width/Height via ConverterParameter if needed.
        private const double DefaultDiameter = 60.0;
        private const double DefaultStrokeThickness = 6.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Parse progress fraction
            double progress = 0;
            if (value != null)
            {
                if (!double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out progress))
                    progress = 0;
            }
            progress = Math.Max(0, Math.Min(1, progress));

            // Compute geometry
            double diameter = DefaultDiameter;
            double thickness = DefaultStrokeThickness;
            double radius = (diameter - thickness) / 2;
            double cx = diameter / 2;
            double cy = diameter / 2;

            // Start at 12 o'clock (-90°)
            double startAngle = -90 * Math.PI / 180;
            double sweepAngle = 2 * Math.PI * progress;
            double endAngle = startAngle + sweepAngle;

            // Points
            var startPoint = new Point(
                cx + radius * Math.Cos(startAngle),
                cy + radius * Math.Sin(startAngle));
            var endPoint = new Point(
                cx + radius * Math.Cos(endAngle),
                cy + radius * Math.Sin(endAngle));

            // Large arc flag
            bool isLargeArc = sweepAngle > Math.PI;

            // Build the PathFigure
            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                Segments = new PathSegmentCollection
                {
                    new ArcSegment
                    {
                        Point         = endPoint,
                        Size          = new Size(radius, radius),
                        SweepDirection= SweepDirection.Clockwise,
                        IsLargeArc    = isLargeArc
                    }
                }
            };

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
