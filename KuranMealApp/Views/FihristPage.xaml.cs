using Microsoft.Maui.Controls;
using KuranMealApp.ViewModels;

namespace KuranMealApp.Views;

public partial class FihristPage : ContentPage
{
    public FihristPage(FihristViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
