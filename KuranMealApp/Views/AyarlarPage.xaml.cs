using Microsoft.Maui.Controls;

namespace KuranMealApp.Views;

public partial class AyarlarPage : ContentPage
{
    public AyarlarPage(ViewModels.AyarlarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnFontCardTapped(object sender, TappedEventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleTo(0.92, 80, Easing.CubicOut);
        await border.ScaleTo(1.0, 80, Easing.CubicIn);
    }

    private async void OnFontScaleTapped(object sender, TappedEventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleTo(0.85, 80, Easing.CubicOut);
        await border.ScaleTo(1.0, 80, Easing.CubicIn);
    }
}
