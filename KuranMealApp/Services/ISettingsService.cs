namespace KuranMealApp.Services;

public class SettingsChangedEventArgs : EventArgs
{
    public string SettingName { get; }
    public object NewValue { get; }

    public SettingsChangedEventArgs(string settingName, object newValue)
    {
        SettingName = settingName;
        NewValue = newValue;
    }
}

public interface ISettingsService
{
    List<string> SelectedTranslators { get; set; }
    bool UseDyslexicFont { get; set; }
    double FontSizeScale { get; set; }
    bool UseHorizontalReadingMode { get; set; }
    bool IsDarkMode { get; set; }
    string SelectedFontFamily { get; set; }
    void ApplySettingsToResources();
    event EventHandler<SettingsChangedEventArgs> SettingsChanged;
}
