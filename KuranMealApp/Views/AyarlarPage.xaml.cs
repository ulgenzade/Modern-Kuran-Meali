using Microsoft.Maui.Controls;

namespace KuranMealApp.Views;

public partial class AyarlarPage : ContentPage
{
    private readonly ViewModels.AyarlarViewModel _viewModel;

    public AyarlarPage(ViewModels.AyarlarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadSettings();
    }

    private async void OnFontCardTapped(object sender, TappedEventArgs e)
    {
        var border = (View)sender;
        await border.ScaleToAsync(0.92, 80, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 80, Easing.CubicIn);
        
        // Sadece animasyon için, komut zaten XAML'da bağlı (ToggleFontCommand)
    }

    private async void OnFontScaleTapped(object sender, TappedEventArgs e)
    {
        var border = (View)sender;
        await border.ScaleToAsync(0.85, 80, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 80, Easing.CubicIn);
    }

    private async void OnApplyTapped(object sender, TappedEventArgs e)
    {
        var border = (View)sender;
        await border.ScaleToAsync(0.92, 80, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 80, Easing.CubicIn);
    }

    private async void OnPillButtonTapped(object sender, TappedEventArgs e)
    {
        var border = (View)sender;
        await border.ScaleToAsync(0.94, 60, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 100, Easing.CubicIn);
    }
}
