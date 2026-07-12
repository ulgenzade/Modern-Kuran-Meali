using System.ComponentModel;
using KuranMealApp.ViewModels;
using KuranMealApp.Models;
using KuranMealApp.Services;

namespace KuranMealApp.Views;

public partial class AyetlerPage : ContentPage
{
    private readonly AyetlerViewModel _viewModel;
    private readonly ISettingsService _settings;
    private double _startY = 0;
    private double _startThumbY = 0;
    private int _lastAyetTargetIndex = -1;
    private CancellationTokenSource? _scrollCts;
    private bool _isSheetPanning = false;
    private const double PanDeadZone = 12; // pixels before pan activates

    public AyetlerPage(AyetlerViewModel viewModel, ISettingsService settings)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _settings = settings;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _viewModel.ScrollToRequested += ViewModel_ScrollToRequested;
        _settings.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if (e.SettingName == nameof(ISettingsService.FontSizeScale) || 
            e.SettingName == nameof(ISettingsService.SelectedFontFamily) ||
            e.SettingName == nameof(ISettingsService.UseDyslexicFont))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_viewModel.IsVerticalMode)
                {
                    var items = AyetListView.ItemsSource;
                    AyetListView.ItemsSource = null;
                    AyetListView.ItemsSource = items;
                }
                else
                {
                    var items = AyetCarouselView.ItemsSource;
                    AyetCarouselView.ItemsSource = null;
                    AyetCarouselView.ItemsSource = items;
                }
            });
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ReloadSettings();
        UpdateScrollbarVisibility();
    }

    protected override bool OnBackButtonPressed()
    {
        if (TefsirOverlay != null && TefsirOverlay.IsVisible)
        {
            _viewModel.ClosePopupsCommand.Execute(null);
            return true;
        }
        if (NuzulOverlay != null && NuzulOverlay.IsVisible)
        {
            _viewModel.ClosePopupsCommand.Execute(null);
            return true;
        }
        if (SurePickerOverlay != null && SurePickerOverlay.IsVisible)
        {
            _viewModel.ToggleSurePickerCommand.Execute(null);
            return true;
        }
        if (AyetPickerOverlay != null && AyetPickerOverlay.IsVisible)
        {
            _viewModel.ToggleAyetPickerCommand.Execute(null);
            return true;
        }
        if (SettingsSheetOverlay != null && SettingsSheetOverlay.IsVisible)
        {
            _viewModel.ToggleSettingsSheetCommand.Execute(null);
            return true;
        }
        if (BottomSheetOverlay != null && BottomSheetOverlay.IsVisible)
        {
            _viewModel.ToggleFilterSheetCommand.Execute(null);
            return true;
        }

        return base.OnBackButtonPressed();
    }

    private void ViewModel_ScrollToRequested(AyetItem item, SureGroup group)
    {
        if (_viewModel.IsVerticalMode)
        {
            AyetListView.ScrollTo(item, group, ScrollToPosition.Start, false);
        }
        else
        {
            AyetCarouselView.ScrollTo(item, null, ScrollToPosition.Center, false);
        }
    }

    private bool _isAyetPanning = false;

    private void OnAyetScrollTrackTapped(object sender, TappedEventArgs e)
    {
        if (_viewModel.FlatAyetler == null || _viewModel.FlatAyetler.Count == 0) return;

        var touchPosition = e.GetPosition((View)sender);
        if (touchPosition != null)
        {
            var yPos = touchPosition.Value.Y;
            var trackHeight = AyetScrollTrackGrid.Height;
            var thumbHeight = AyetScrollThumbGrid.Height;
            var maxThumbY = trackHeight - thumbHeight;

            // Center thumb on tap
            var newY = yPos - (thumbHeight / 2);
            newY = Math.Max(0, Math.Min(maxThumbY, newY));

            // Scroll the list
            var scrollPercent = newY / maxThumbY;
            var targetIndex = (int)(scrollPercent * (_viewModel.FlatAyetler.Count - 1));

            if (targetIndex >= 0 && targetIndex < _viewModel.FlatAyetler.Count)
            {
                _lastAyetTargetIndex = targetIndex;
                var item = _viewModel.FlatAyetler[targetIndex];
                var group = _viewModel.AyetGroups.FirstOrDefault(g => g.Contains(item));
                if (group != null)
                {
                    AyetListView.ScrollTo(item, group, position: ScrollToPosition.Start, animate: false);
                }
            }
        }
    }

    private void AyetListView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (!_viewModel.IsVerticalMode) return;
        
        int index = e.FirstVisibleItemIndex;
        
        if (index > 0)
        {
            int itemIndex = index - 1;
            if (itemIndex >= 0 && itemIndex < _viewModel.FlatAyetler.Count)
            {
                var item = _viewModel.FlatAyetler[itemIndex];
                _viewModel.CurrentAyetNo = item.AyetNo;
                _viewModel.CurrentAyetId = item.AyetId;
            }
        }

        // Update scrollbar thumb — skip if user is actively panning the thumb
        if (!_isAyetPanning && _viewModel.FlatAyetler.Count > 1)
        {
            var trackHeight = AyetScrollTrackGrid.Height;
            var thumbHeight = AyetScrollThumbGrid.Height;
            var maxNewY = trackHeight - thumbHeight;
            if (maxNewY > 0)
            {
                int itemIndex = index > 0 ? index - 1 : 0;
                double pct = (double)itemIndex / (_viewModel.FlatAyetler.Count - 1);
                if (pct < 0) pct = 0;
                if (pct > 1) pct = 1;
                AyetScrollThumbGrid.TranslationY = pct * maxNewY;
            }
        }
    }

    private void OnAyetScrollThumbPan(object sender, PanUpdatedEventArgs e)
    {
        if (_viewModel.FlatAyetler == null || _viewModel.FlatAyetler.Count == 0) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isAyetPanning = true;
                // Başlangıç anındaki konumu al (Zıplamayı önler)
                _startThumbY = AyetScrollThumbGrid.TranslationY;
                break;
            case GestureStatus.Running:
                var trackHeight = AyetScrollTrackGrid.Height;
                var thumbHeight = AyetScrollThumbGrid.Height;
                var maxNewY = trackHeight - thumbHeight;
                if (maxNewY <= 0) return;

                // Başlangıç pozisyonuna göre eklentiyi yap
                var newY = _startThumbY + e.TotalY;
                if (newY < 0) newY = 0;
                if (newY > maxNewY) newY = maxNewY;

                AyetScrollThumbGrid.TranslationY = newY;

                var pct = newY / maxNewY;
                int targetIndex = (int)(pct * (_viewModel.FlatAyetler.Count - 1));
                if (targetIndex != _lastAyetTargetIndex && targetIndex >= 0 && targetIndex < _viewModel.FlatAyetler.Count)
                {
                    _lastAyetTargetIndex = targetIndex;
                    var item = _viewModel.FlatAyetler[targetIndex];
                    var group = _viewModel.AyetGroups.FirstOrDefault(g => g.Contains(item));
                    if (group != null)
                    {
                        AyetListView.ScrollTo(item, group, ScrollToPosition.Start, false);
                    }
                }
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isAyetPanning = false;
                break;
        }
    }

    private void OnCarouselCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is AyetItem item)
        {
            _viewModel.CurrentAyetNo = item.AyetNo;
            _viewModel.CurrentAyetId = item.AyetId;
        }
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

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(AyetlerViewModel.ShowFilterSheet) ||
                e.PropertyName == nameof(AyetlerViewModel.ShowSettingsSheet) ||
                e.PropertyName == nameof(AyetlerViewModel.ShowSurePicker) ||
                e.PropertyName == nameof(AyetlerViewModel.ShowAyetPicker) ||
                e.PropertyName == nameof(AyetlerViewModel.IsTefsirPopupVisible) ||
                e.PropertyName == nameof(AyetlerViewModel.IsNuzulPopupVisible) ||
                e.PropertyName == nameof(AyetlerViewModel.IsVerticalMode))
            {
                UpdateScrollbarVisibility();
            }

            if (e.PropertyName == nameof(AyetlerViewModel.SureNo))
            {
                _lastAyetTargetIndex = -1;
            }

            if (e.PropertyName == nameof(AyetlerViewModel.ShowFilterSheet))
            {
                if (_viewModel.ShowFilterSheet)
                {
                    ConfigureSheetHeight(BottomSheetContainer, 440, out double defaultY, out double closedY);
                    BottomSheetContainer.TranslationY = closedY;
                    BottomSheetOverlay.Opacity = 0;
                    BottomSheetOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await BottomSheetOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await BottomSheetContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
                }
                else
                {
                    ConfigureSheetHeight(BottomSheetContainer, 440, out double defaultY, out double closedY);
                    _ = BottomSheetOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await BottomSheetContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                    BottomSheetOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.ShowSettingsSheet))
            {
                if (_viewModel.ShowSettingsSheet)
                {
                    ConfigureSheetHeight(SettingsSheetContainer, 320, out double defaultY, out double closedY);
                    SettingsSheetContainer.TranslationY = closedY;
                    SettingsSheetOverlay.Opacity = 0;
                    SettingsSheetOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await SettingsSheetOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await SettingsSheetContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
                }
                else
                {
                    ConfigureSheetHeight(SettingsSheetContainer, 320, out double defaultY, out double closedY);
                    _ = SettingsSheetOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await SettingsSheetContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                    SettingsSheetOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.ShowSurePicker))
            {
                if (_viewModel.ShowSurePicker)
                {
                    ConfigureSheetHeight(SurePickerContainer, 440, out double defaultY, out double closedY);
                    SurePickerContainer.TranslationY = closedY;
                    SurePickerOverlay.Opacity = 0;
                    SurePickerOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await SurePickerOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await SurePickerContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
                }
                else
                {
                    ConfigureSheetHeight(SurePickerContainer, 440, out double defaultY, out double closedY);
                    _ = SurePickerOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await SurePickerContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                    SurePickerOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.ShowAyetPicker))
            {
                if (_viewModel.ShowAyetPicker)
                {
                    AyetPickerCard.Scale = 0.85;
                    AyetPickerOverlay.Opacity = 0;
                    AyetPickerOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await AyetPickerOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await AyetPickerCard.ScaleToAsync(1.0, 300, Easing.SpringOut);
                }
                else
                {
                    _ = AyetPickerOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await AyetPickerCard.ScaleToAsync(0.85, 250, Easing.CubicIn);
                    AyetPickerOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.IsTefsirPopupVisible))
            {
                if (_viewModel.IsTefsirPopupVisible)
                {
                    ConfigureSheetHeight(TefsirSheetContainer, 440, out double defaultY, out double closedY);
                    TefsirSheetContainer.TranslationY = closedY;
                    UpdateScrollViewPadding(TefsirScrollView, defaultY);
                    TefsirOverlay.Opacity = 0;
                    TefsirOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await TefsirOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await TefsirSheetContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
                }
                else
                {
                    ConfigureSheetHeight(TefsirSheetContainer, 440, out double defaultY, out double closedY);
                    _ = TefsirOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await TefsirSheetContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                    TefsirOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.IsNuzulPopupVisible))
            {
                if (_viewModel.IsNuzulPopupVisible)
                {
                    ConfigureSheetHeight(NuzulSheetContainer, 340, out double defaultY, out double closedY);
                    NuzulSheetContainer.TranslationY = closedY;
                    UpdateScrollViewPadding(NuzulScrollView, defaultY);
                    NuzulOverlay.Opacity = 0;
                    NuzulOverlay.IsVisible = true;
                    await Task.Delay(50);
                    await NuzulOverlay.FadeToAsync(1, 250, Easing.SinOut);
                    await NuzulSheetContainer.TranslateToAsync(0, defaultY, 300, Easing.CubicOut);
                }
                else
                {
                    ConfigureSheetHeight(NuzulSheetContainer, 340, out double defaultY, out double closedY);
                    _ = NuzulOverlay.FadeToAsync(0, 200, Easing.SinIn);
                    await NuzulSheetContainer.TranslateToAsync(0, closedY, 250, Easing.CubicIn);
                    NuzulOverlay.IsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(AyetlerViewModel.UseHorizontalReadingMode))
            {
                var currentAyetNo = _viewModel.CurrentAyetNo;
                var item = _viewModel.FlatAyetler.FirstOrDefault(a => a.AyetNo == currentAyetNo);
                if (item != null)
                {
                    await Task.Delay(100);
                    if (_viewModel.IsVerticalMode)
                    {
                        var firstGroup = _viewModel.AyetGroups.FirstOrDefault();
                        if (firstGroup != null)
                        {
                            AyetListView.ScrollTo(item, firstGroup, ScrollToPosition.Start, false);
                        }
                    }
                    else
                    {
                        AyetCarouselView.ScrollTo(item, null, ScrollToPosition.Center, false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Animation error: {ex}");
            if (e.PropertyName == nameof(AyetlerViewModel.ShowFilterSheet)) BottomSheetOverlay.IsVisible = _viewModel.ShowFilterSheet;
            if (e.PropertyName == nameof(AyetlerViewModel.ShowSettingsSheet)) SettingsSheetOverlay.IsVisible = _viewModel.ShowSettingsSheet;
            if (e.PropertyName == nameof(AyetlerViewModel.ShowSurePicker)) SurePickerOverlay.IsVisible = _viewModel.ShowSurePicker;
            if (e.PropertyName == nameof(AyetlerViewModel.ShowAyetPicker)) AyetPickerOverlay.IsVisible = _viewModel.ShowAyetPicker;
            if (e.PropertyName == nameof(AyetlerViewModel.IsTefsirPopupVisible)) TefsirOverlay.IsVisible = _viewModel.IsTefsirPopupVisible;
            if (e.PropertyName == nameof(AyetlerViewModel.IsNuzulPopupVisible)) NuzulOverlay.IsVisible = _viewModel.IsNuzulPopupVisible;
        }
    }

private async void OnBackClicked(object sender, EventArgs e)
    {
        var button = (VisualElement)sender;
        await button.ScaleTo(0.9, 80, Easing.CubicOut);
        await button.ScaleTo(1.0, 80, Easing.CubicIn);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnPrevSureClicked(object sender, EventArgs e)
    {
        var button = (VisualElement)sender;
        await button.ScaleTo(0.9, 80, Easing.CubicOut);
        await button.ScaleTo(1.0, 80, Easing.CubicIn);
        if (_viewModel.NavigatePrevSureCommand.CanExecute(null))
        {
            _viewModel.NavigatePrevSureCommand.Execute(null);
        }
    }

    private async void OnNextSureClicked(object sender, EventArgs e)
    {
        var button = (VisualElement)sender;
        await button.ScaleTo(0.9, 80, Easing.CubicOut);
        await button.ScaleTo(1.0, 80, Easing.CubicIn);
        if (_viewModel.NavigateNextSureCommand.CanExecute(null))
        {
            _viewModel.NavigateNextSureCommand.Execute(null);
        }
    }

    private async void OnToggleOptionTapped(object sender, EventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleTo(0.9, 80, Easing.CubicOut);
        await border.ScaleTo(1.0, 80, Easing.CubicIn);
    }

    private void UpdateScrollbarVisibility()
    {
        if (_viewModel == null || AyetScrollTrackGrid == null) return;
        
        bool anyOverlayVisible = _viewModel.ShowFilterSheet || 
                                 _viewModel.ShowSettingsSheet || 
                                 _viewModel.ShowSurePicker || 
                                 _viewModel.ShowAyetPicker || 
                                 _viewModel.IsTefsirPopupVisible || 
                                 _viewModel.IsNuzulPopupVisible;
                                 
        AyetScrollTrackGrid.IsVisible = _viewModel.IsVerticalMode && !anyOverlayVisible;
        Shell.SetTabBarIsVisible(this, !anyOverlayVisible);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel.ScrollToRequested -= ViewModel_ScrollToRequested;
        _settings.SettingsChanged -= OnSettingsChanged;
    }

    private async void OnAyetNumberTapped(object sender, TappedEventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleTo(0.82, 80, Easing.CubicOut);
        await border.ScaleTo(1.0, 80, Easing.CubicIn);
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
                // Dead zone: ignore tiny movements to prevent jitter
                if (Math.Abs(e.TotalY) < PanDeadZone && !_isSheetPanning)
                    return;
                _isSheetPanning = true;
                
                double targetY = _startY + e.TotalY;
                if (targetY < 0) targetY = 0;
                if (targetY > closedTranslationY) targetY = closedTranslationY;
                container.TranslationY = targetY;
                
                if (container == TefsirSheetContainer) UpdateScrollViewPadding(TefsirScrollView, targetY);
                else if (container == NuzulSheetContainer) UpdateScrollViewPadding(NuzulScrollView, targetY);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isSheetPanning = false;
                double currentY = container.TranslationY;
                // Cancel any previous animation on this container
                container.CancelAnimations();
                
                if (currentY > defaultTranslationY + 80)
                {
                    var cts = new CancellationTokenSource();
                    _scrollCts?.Cancel();
                    _scrollCts = cts;
                    container.TranslateToAsync(0, closedTranslationY, 250, Easing.CubicIn).ContinueWith(t => 
                    {
                        if (!cts.Token.IsCancellationRequested)
                        {
                            MainThread.BeginInvokeOnMainThread(() => {
                                container.TranslationY = closedTranslationY;
                                closeAction.Invoke();
                            });
                        }
                    });
                }
                else if (currentY < defaultTranslationY - 80)
                {
                    container.TranslateToAsync(0, 0, 250, Easing.CubicOut);
                    if (container == TefsirSheetContainer) UpdateScrollViewPadding(TefsirScrollView, 0);
                    else if (container == NuzulSheetContainer) UpdateScrollViewPadding(NuzulScrollView, 0);
                }
                else
                {
                    container.TranslateToAsync(0, defaultTranslationY, 200, Easing.CubicOut);
                    if (container == TefsirSheetContainer) UpdateScrollViewPadding(TefsirScrollView, defaultTranslationY);
                    else if (container == NuzulSheetContainer) UpdateScrollViewPadding(NuzulScrollView, defaultTranslationY);
                }
                break;
        }
    }

    private void UpdateScrollViewPadding(ScrollView? scrollView, double translationY)
    {
        if (scrollView == null) return;
        scrollView.Padding = new Thickness(20, 0, 20, 20 + translationY);
    }

    private async void OnSheetButtonTapped(object sender, TappedEventArgs e)
    {
        var border = (VisualElement)sender;
        await border.ScaleTo(0.92, 80, Easing.CubicOut);
        await border.ScaleTo(1.0, 80, Easing.CubicIn);
    }

    private void OnSurePickerSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(SurePickerContainer, 440, out double defaultY, out double closedY);
        HandlePan(SurePickerContainer, e, defaultY, closedY, () => _viewModel.ToggleSurePickerCommand.Execute(null));
    }

    private void OnSettingsSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(SettingsSheetContainer, 320, out double defaultY, out double closedY);
        HandlePan(SettingsSheetContainer, e, defaultY, closedY, () => _viewModel.ToggleSettingsSheetCommand.Execute(null));
    }

    private void OnBottomSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(BottomSheetContainer, 440, out double defaultY, out double closedY);
        HandlePan(BottomSheetContainer, e, defaultY, closedY, () => _viewModel.ToggleFilterSheetCommand.Execute(null));
    }

    private void OnTefsirSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(TefsirSheetContainer, 440, out double defaultY, out double closedY);
        HandlePan(TefsirSheetContainer, e, defaultY, closedY, () => _viewModel.ClosePopupsCommand.Execute(null));
    }

    private void OnNuzulSheetPan(object sender, PanUpdatedEventArgs e)
    {
        ConfigureSheetHeight(NuzulSheetContainer, 340, out double defaultY, out double closedY);
        HandlePan(NuzulSheetContainer, e, defaultY, closedY, () => _viewModel.ClosePopupsCommand.Execute(null));
    }
}
