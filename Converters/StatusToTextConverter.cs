using System.Globalization;
using System.Windows.Data;
using RouterMeter.Models;

namespace RouterMeter.Converters;

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ApiConnectionStatus status
            ? status switch
            {
                ApiConnectionStatus.Online => "API Status: Online",
                ApiConnectionStatus.Offline => "API Status: Offline",
                _ => "API Status: Connecting..."
            }
            : "API Status: --";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
