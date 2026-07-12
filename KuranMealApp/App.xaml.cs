namespace KuranMealApp;

public partial class App : Application
{
    public App(KuranMealApp.Services.ISettingsService settingsService)
    {
        InitializeComponent();
        
        // Temayı başlangıçta ayarla
        UserAppTheme = settingsService.IsDarkMode ? AppTheme.Dark : AppTheme.Light;

        // Font ve boyut ayarlarını başlangıçta uygula
        settingsService.ApplySettingsToResources();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
