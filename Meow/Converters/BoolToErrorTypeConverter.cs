namespace Meow.Converters;

/// <summary>
/// Converts the IsNoInternet boolean to the appropriate ErrorDisplayType.
/// When true: NoInternet, when false: ApiError.
/// </summary>
public class BoolToErrorTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isNoInternet && isNoInternet)
            return Views.ErrorDisplayType.NoInternet;

        return Views.ErrorDisplayType.ApiError;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
