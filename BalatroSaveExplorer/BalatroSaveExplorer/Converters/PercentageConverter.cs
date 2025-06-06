using System.Globalization;
using System.Windows.Data;

namespace BalatroSaveExplorer.Converters
{
  /// <summary>
  /// Multiplies the incoming double (ActualWidth) by the parameter (e.g. 0.85 for 85%).
  /// </summary>
  public class PercentageConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double actualWidth &&
          parameter is string paramText &&
          double.TryParse(paramText, NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
      {
        return actualWidth * pct;
      }

      return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
  }
}