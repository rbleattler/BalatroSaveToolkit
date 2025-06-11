using System;
using System.Globalization;
using System.Windows.Data;

namespace BalatroSaveExplorer.Converters;

/// <summary>
/// Converter that determines the backup type based on the filename
/// </summary>
public class BackupTypeConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is string filename)
    {
      if (filename.Contains("_auto"))
        return "Auto";
      if (filename.Contains("_manual"))
        return "Manual";
      if (filename.Contains("pre_restore"))
        return "Pre-Restore";
      return "Manual"; // Default assumption
    }
    return "Unknown";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
