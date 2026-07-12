using Microsoft.Maui.Controls;
using KuranMealApp.ViewModels;

namespace KuranMealApp.Views;

public partial class AramaPage : ContentPage
{
    public AramaPage(AramaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
