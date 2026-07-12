using System.Globalization;

namespace KuranMealApp.ViewModels;

public class BoolToModeStringConverter : IValueConverter
{
    public static readonly BoolToModeStringConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isHorizontal)
        {
            return isHorizontal ? "Yatay" : "Dikey";
        }
        return "Dikey";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
