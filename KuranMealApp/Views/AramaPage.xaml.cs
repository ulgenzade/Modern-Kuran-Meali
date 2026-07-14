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

    private double _startY = 0;
    private bool _isSheetPanning = false;
    private const double PanDeadZone = 12;

    private async void OnPillButtonTapped(object sender, TappedEventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleToAsync(0.94, 60, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 100, Easing.CubicIn);
    }

    private void ConfigureSheetHeight(Border container, double defaultVisibleHeight, out double defaultTranslationY, out double closedTranslationY)
    {
        double pageHeight = this.Height;
        if (pageHeight <= 0) pageHeight = 700; // Fallback
        
        double maxHeight = pageHeight * 0.82;
        if (Math.Abs(container.HeightRequest - maxHeight) > 1)
        {
            container.HeightRequest = maxHeight;
        }
        
        closedTranslationY = maxHeight;
        defaultTranslationY = maxHeight - defaultVisibleHeight;
        if (defaultTranslationY < 0) defaultTranslationY = 0;
    }

    private void HandlePan(Border container, PanUpdatedEventArgs e, double defaultTranslationY, double closedTranslationY, Action closeAction)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = container.TranslationY;
                _isSheetPanning = true;
                break;
            case GestureStatus.Running:
                if (Math.Abs(e.TotalY) < PanDeadZone && !_isSheetPanning)
                    return;
                _isSheetPanning = true;
                
                double targetY = _startY + e.TotalY;
                if (targetY < 0) targetY = 0;
                if (targetY > closedTranslationY) targetY = closedTranslationY;
                container.TranslationY = targetY;
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isSheetPanning = false;
                double currentY = container.TranslationY;
                container.CancelAnimations();
                
                if (currentY > defaultTranslationY + 80)
                {
                    container.TranslateToAsync(0, closedTranslationY, 250, Easing.CubicIn).ContinueWith(t => 
                    {
                        MainThread.BeginInvokeOnMainThread(() => {
                            container.TranslationY = closedTranslationY;
                            closeAction.Invoke();
                        });
                    });
                }
                else if (currentY < defaultTranslationY - 80)
                {
                    container.TranslateToAsync(0, 0, 250, Easing.CubicOut);
                }
                else
                {
                    container.TranslateToAsync(0, defaultTranslationY, 200, Easing.CubicOut);
                }
                break;
        }
    }

    void OnTranslatorPickerSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(TranslatorPickerContainer, 440, out double defaultY, out double closedY);
        HandlePan(TranslatorPickerContainer, e, defaultY, closedY, () => {
            var vm = (AramaViewModel)BindingContext;
            if (vm.IsTranslatorPickerVisible) vm.ToggleTranslatorPickerCommand.Execute(null);
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (AramaViewModel)BindingContext;
        vm.PropertyChanged += Vm_PropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        var vm = (AramaViewModel)BindingContext;
        vm.PropertyChanged -= Vm_PropertyChanged;
    }

    private async void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AramaViewModel.IsTranslatorPickerVisible))
        {
            var vm = (AramaViewModel)BindingContext;
            if (vm.IsTranslatorPickerVisible)
            {
                ConfigureSheetHeight(TranslatorPickerContainer, 440, out double defaultY, out double closedY);
                TranslatorPickerContainer.TranslationY = closedY;
                TranslatorPickerOverlay.Opacity = 0;
                TranslatorPickerOverlay.IsVisible = true;
                await Task.Delay(50);
                await TranslatorPickerOverlay.FadeToAsync(1, 250, Easing.SinOut);
                await TranslatorPickerContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
            }
            else
            {
                ConfigureSheetHeight(TranslatorPickerContainer, 440, out double defaultY, out double closedY);
                _ = TranslatorPickerOverlay.FadeToAsync(0, 200, Easing.SinIn);
                await TranslatorPickerContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                TranslatorPickerOverlay.IsVisible = false;
            }
        }
    }
}
