using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KuranMealApp.Services;

namespace KuranMealApp.ViewModels;

public class AyarlarViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settings;

    private bool _isDarkMode;
    private bool _useDyslexicFont;
    private double _fontSizeScale;

    public AyarlarViewModel(ISettingsService settings)
    {
        _settings = settings;
        
        _isDarkMode = _settings.IsDarkMode;
        _useDyslexicFont = _settings.UseDyslexicFont;
        _fontSizeScale = _settings.FontSizeScale;

        IncreaseFontSizeCommand = new Command(() => { if (FontSizeScale < 2.0) FontSizeScale = Math.Round(FontSizeScale + 0.1, 1); });
        DecreaseFontSizeCommand = new Command(() => { if (FontSizeScale > 0.7) FontSizeScale = Math.Round(FontSizeScale - 0.1, 1); });
        ApplyCommand = new Command(ApplySettings);
    }

    public void LoadSettings()
    {
        _isDarkMode = _settings.IsDarkMode;
        _useDyslexicFont = _settings.UseDyslexicFont;
        _fontSizeScale = _settings.FontSizeScale;

        OnPropertyChanged(nameof(IsDarkMode));
        OnPropertyChanged(nameof(UseDyslexicFont));
        OnPropertyChanged(nameof(FontSizeScale));
        OnPropertyChanged(nameof(FontSizeDisplay));
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set { if (_isDarkMode != value) { _isDarkMode = value; OnPropertyChanged(); } }
    }

    public bool UseDyslexicFont
    {
        get => _useDyslexicFont;
        set { if (_useDyslexicFont != value) { _useDyslexicFont = value; OnPropertyChanged(); } }
    }

    public double FontSizeScale
    {
        get => _fontSizeScale;
        set { if (_fontSizeScale != value) { _fontSizeScale = value; OnPropertyChanged(); OnPropertyChanged(nameof(FontSizeDisplay)); } }
    }

    public string FontSizeDisplay => $"x{FontSizeScale:F1}";

    public ICommand IncreaseFontSizeCommand { get; }
    public ICommand DecreaseFontSizeCommand { get; }
    public ICommand ApplyCommand { get; }

    private void ApplySettings()
    {
        _settings.IsDarkMode = IsDarkMode;
        _settings.UseDyslexicFont = UseDyslexicFont;
        _settings.FontSizeScale = FontSizeScale;
        _settings.ApplySettingsToResources();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}