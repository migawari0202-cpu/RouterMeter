using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RouterMeter.Models;

namespace RouterMeter.Converters;

public class StatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Online = new(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly SolidColorBrush Offline = new(Color.FromRgb(0xEF, 0x53, 0x50));
    private static readonly SolidColorBrush Connecting = new(Color.FromRgb(0x9E, 0x9E, 0x9E));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ApiConnectionStatus status
            ? status switch
            {
                ApiConnectionStatus.Online => Online,
                ApiConnectionStatus.Offline => Offline,
                _ => Connecting
            }
            : Connecting;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
