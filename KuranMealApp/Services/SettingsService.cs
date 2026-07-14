using Microsoft.Maui.Storage;

namespace KuranMealApp.Services;

public class SettingsService : ISettingsService
{
    private const string KeyTranslators = "selected_translators";
    private const string KeyDyslexic = "use_dyslexic_font";
    private const string KeyFontSize = "font_size_scale";
    private const string KeyReadingMode = "use_horizontal_reading_mode";

    private const string DefaultTranslators = "Diyanet İşleri Meali (Yeni)";

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    protected virtual void OnSettingsChanged(string settingName, object newValue)
    {
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(settingName, newValue));
    }

    public List<string> SelectedTranslators
    {
        get
        {
            var raw = Preferences.Default.Get(KeyTranslators, DefaultTranslators);
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        set
        {
            var raw = string.Join("|", value);
            Preferences.Default.Set(KeyTranslators, raw);
            OnSettingsChanged(nameof(SelectedTranslators), value);
        }
    }

    public bool UseDyslexicFont
    {
        get => Preferences.Default.Get(KeyDyslexic, false);
        set
        {
            Preferences.Default.Set(KeyDyslexic, value);
            OnSettingsChanged(nameof(UseDyslexicFont), value);
            ApplySettingsToResources();
        }
    }

    public double FontSizeScale
    {
        get => Preferences.Default.Get(KeyFontSize, 1.0);
        set
        {
            Preferences.Default.Set(KeyFontSize, value);
            OnSettingsChanged(nameof(FontSizeScale), value);
            ApplySettingsToResources();
        }
    }

    public bool UseHorizontalReadingMode
    {
        get => Preferences.Default.Get(KeyReadingMode, false);
        set
        {
            Preferences.Default.Set(KeyReadingMode, value);
            OnSettingsChanged(nameof(UseHorizontalReadingMode), value);
        }
    }

    public bool IsDarkMode
    {
        get => Preferences.Default.Get("is_dark_mode", false);
        set
        {
            Preferences.Default.Set("is_dark_mode", value);
            OnSettingsChanged(nameof(IsDarkMode), value);
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            }
        }
    }

    public void ApplySettingsToResources()
    {
        if (Application.Current == null) return;

        var resources = Application.Current.Resources;

        resources["AppFontFamily"] = UseDyslexicFont ? "OpenDyslexic" : "OpenSans";

        double scale = FontSizeScale;
        resources["AppFontSizeBody"] = 15 * scale;
        resources["AppFontSizeTitle"] = 16 * scale;
        resources["AppFontSizeHeadline"] = 18 * scale;
        resources["AppFontSizeHeadlineLarge"] = 22 * scale;
    }
}