using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BalatroSaveExplorer.Services;

namespace BalatroSaveExplorer.Converters
{
    public class ErrorForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isError = value is bool b && b;
            if (isError)
            {
                return new SolidColorBrush(Colors.Red);
            }

            // Try to get the StatusBarTextBrush from current application resources
            try
            {
                var app = System.Windows.Application.Current;
                if (app?.Resources != null)
                {
                    // Try to find the resource in the merged dictionaries
                    object resource = app.TryFindResource("StatusBarTextBrush");
                    if (resource is SolidColorBrush brush)
                    {
                        return brush;
                    }
                }
            }
            catch
            {
                // Ignore any exceptions during resource lookup
            }

            // Fallback to appropriate color based on theme detection
            try
            {
                var app = System.Windows.Application.Current;
                if (app?.Resources != null)
                {
                    // Try to detect if we're in dark theme by checking background
                    object bgResource = app.TryFindResource("StatusBarBackgroundBrush");
                    if (bgResource is SolidColorBrush bgBrush)
                    {
                        var color = bgBrush.Color;
                        // If background is dark, use light text
                        if (color.R + color.G + color.B < 384) // Sum less than ~50% of max (255*3)
                        {
                            return new SolidColorBrush(Colors.White);
                        }
                        else
                        {
                            return new SolidColorBrush(Colors.Black);
                        }
                    }
                }
            }
            catch
            {
                // Ignore any exceptions
            }

            // Final fallback
            return new SolidColorBrush(Colors.White);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BalatroStateToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is BalatroState state)
            {
                return state switch
                {
                    BalatroState.Running => Brushes.LimeGreen,
                    BalatroState.NotRunning => Brushes.Red,
                    _ => Brushes.Gold
                };
            }
            return Brushes.Gray;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
