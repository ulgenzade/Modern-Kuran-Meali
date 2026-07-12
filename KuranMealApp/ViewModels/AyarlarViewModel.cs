using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KuranMealApp.Services;

namespace KuranMealApp.ViewModels;

public class AyarlarViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settings;

    public AyarlarViewModel(ISettingsService settings)
    {
        _settings = settings;
        IncreaseFontSizeCommand = new Command(() => { if (FontSizeScale < 2.0) FontSizeScale = Math.Round(FontSizeScale + 0.1, 1); });
        DecreaseFontSizeCommand = new Command(() => { if (FontSizeScale > 0.7) FontSizeScale = Math.Round(FontSizeScale - 0.1, 1); });
        ToggleOptionCommand = new Command<string>(font => SelectedFontFamily = font);
    }

    public bool IsDarkMode
    {
        get => _settings.IsDarkMode;
        set
        {
            if (_settings.IsDarkMode != value)
            {
                _settings.IsDarkMode = value;
                OnPropertyChanged();
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            }
        }
    }

    public bool UseDyslexicFont
    {
        get => _settings.UseDyslexicFont;
        set
        {
            if (_settings.UseDyslexicFont != value)
            {
                _settings.UseDyslexicFont = value;
                OnPropertyChanged();
            }
        }
    }

    public bool UseHorizontalReadingMode
    {
        get => _settings.UseHorizontalReadingMode;
        set
        {
            if (_settings.UseHorizontalReadingMode != value)
            {
                _settings.UseHorizontalReadingMode = value;
                OnPropertyChanged();
            }
        }
    }

    public double FontSizeScale
    {
        get => _settings.FontSizeScale;
        set
        {
            if (_settings.FontSizeScale != value)
            {
                _settings.FontSizeScale = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FontSizeDisplay));
            }
        }
    }

    public string FontSizeDisplay => $"x{FontSizeScale:0.0}";

    public string SelectedFontFamily
    {
        get => _settings.SelectedFontFamily;
        set
        {
            if (_settings.SelectedFontFamily != value)
            {
                _settings.SelectedFontFamily = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFontOpenSans));
                OnPropertyChanged(nameof(IsFontInter));
                OnPropertyChanged(nameof(IsFontLora));
            }
        }
    }

    public bool IsFontOpenSans
    {
        get => SelectedFontFamily == "OpenSans";
        set { if (value) SelectedFontFamily = "OpenSans"; }
    }

    public bool IsFontInter
    {
        get => SelectedFontFamily == "Inter";
        set { if (value) SelectedFontFamily = "Inter"; }
    }

    public bool IsFontLora
    {
        get => SelectedFontFamily == "Lora";
        set { if (value) SelectedFontFamily = "Lora"; }
    }

    public ICommand IncreaseFontSizeCommand { get; }
    public ICommand DecreaseFontSizeCommand { get; }
    public ICommand ToggleOptionCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
