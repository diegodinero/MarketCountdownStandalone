using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MarketCountdownApp
{
    public class ProgressToArcConverter : IValueConverter
    {
        // value: a double between 0.0 and 1.0
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Safely unwrap the input
            double progress = 0.0;
            if (value is double d)
            {
                progress = Math.Max(0.0, Math.Min(1.0, d));
            }

            // No arc for zero progress
            if (progress <= 0.0)
                return Geometry.Empty;

            // Compute the sweep angle
            double angle = 360.0 * progress;

            // Radius = half of 60px minus half of 6px stroke
            double radius = 30.0 - 3.0;
            // Start at top (-90°)
            double radians = (Math.PI / 180.0) * (angle - 90.0);

            // Center of our 60×60 control
            Point center = new Point(30.0, 30.0);
            // Starting point (top center)
            Point start = new Point(center.X, center.Y - radius);
            // End point on the circle
            Point end = new Point(
                center.X + radius * Math.Cos(radians),
                center.Y + radius * Math.Sin(radians));

            bool largeArc = angle > 180.0;

            // Create the arc segment
            ArcSegment arc = new ArcSegment
            {
                Point = end,
                Size = new Size(radius, radius),
                RotationAngle = 0,
                IsLargeArc = largeArc,
                SweepDirection = SweepDirection.Clockwise,
                IsStroked = true
            };

            // Build the path figure
            PathFigure figure = new PathFigure
            {
                StartPoint = start,
                IsClosed = false,
                Segments = new PathSegmentCollection { arc }
            };

            // Wrap in a geometry
            PathGeometry geometry = new PathGeometry
            {
                Figures = new PathFigureCollection { figure }
            };

            return geometry;
        }

        // Not used in one‐way binding
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
