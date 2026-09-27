namespace Meow.Converters;

/// <summary>
/// Returns true when the value is not null and not an empty string.
/// Used to show/hide UI elements based on status messages.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string str)
            return !string.IsNullOrEmpty(str);

        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
