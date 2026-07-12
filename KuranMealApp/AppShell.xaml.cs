using KuranMealApp.Views;

namespace KuranMealApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(AyetlerPage), typeof(AyetlerPage));
    }
}
