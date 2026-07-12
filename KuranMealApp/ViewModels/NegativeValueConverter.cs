using System.Globalization;

namespace KuranMealApp.ViewModels;

public class NegativeValueConverter : IValueConverter
{
    public static readonly NegativeValueConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return -d;
        }
        if (value is float f)
        {
            return -f;
        }
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return -d;
        }
        if (value is float f)
        {
            return -f;
        }
        return 0.0;
    }
}
