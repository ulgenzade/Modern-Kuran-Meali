using KuranMealApp.ViewModels;
using KuranMealApp.Models;
using KuranMealApp.Services;

namespace KuranMealApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly ISettingsService _settings;

    public MainPage(MainViewModel viewModel, ISettingsService settings)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _settings = settings;
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
                var items = SureListView.ItemsSource;
                SureListView.ItemsSource = null;
                SureListView.ItemsSource = items;
            });
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (KuranMealApp.Services.CrashLogger.HasCrashLog())
        {
            var crashDetails = KuranMealApp.Services.CrashLogger.ReadCrashLog();
            KuranMealApp.Services.CrashLogger.ClearCrashLog();
            
            await DisplayAlert("Uygulama Hata Raporu", 
                "Son oturumda bir hata oluştu ve uygulama kapandı. Lütfen aşağıdaki bilgiyi kopyalayıp gönderin:\n\n" + crashDetails, 
                "Kapat");
        }

        await _viewModel.LoadSurelerAsync();
    }

    private async void OnSureSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Sure selectedSure)
        {
            // Reset selection
            ((CollectionView)sender).SelectedItem = null;
            
            // Navigate to AyetlerPage passing SureNo query parameter
            await Shell.Current.GoToAsync($"{nameof(AyetlerPage)}?SureNo={selectedSure.SureNo}");
        }
    }

    private bool _isPanning = false;
    private double _startThumbY = 0;
    private int _lastMainTargetIndex = -1;

    private void OnScrollTrackTapped(object sender, TappedEventArgs e)
    {
        if (_viewModel.Sureler == null || _viewModel.Sureler.Count == 0) return;

        var touchPosition = e.GetPosition((View)sender);
        if (touchPosition != null)
        {
            var yPos = touchPosition.Value.Y;
            var trackHeight = ScrollTrackGrid.Height;
            var thumbHeight = ScrollThumbGrid.Height;
            var maxThumbY = trackHeight - thumbHeight;

            // Center thumb on tap
            var newY = yPos - (thumbHeight / 2);
            newY = Math.Max(0, Math.Min(maxThumbY, newY));

            // Scroll the list
            var scrollPercent = newY / maxThumbY;
            var targetIndex = (int)(scrollPercent * (_viewModel.Sureler.Count - 1));

            if (targetIndex >= 0 && targetIndex < _viewModel.Sureler.Count)
            {
                _lastMainTargetIndex = targetIndex;
                SureListView.ScrollTo(targetIndex, position: ScrollToPosition.Start, animate: false);
            }
        }
    }

    private void OnScrollThumbPan(object sender, PanUpdatedEventArgs e)
    {
        if (_viewModel.Sureler == null || _viewModel.Sureler.Count == 0) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isPanning = true;
                // Başlangıç anındaki konumu al (Zıplamayı önler)
                _startThumbY = ScrollThumbGrid.TranslationY;
                break;

            case GestureStatus.Running:
                var trackHeight = SureListView.Height;
                var thumbHeight = ScrollThumbGrid.Height;
                var maxThumbY = trackHeight - thumbHeight;

                // Başlangıç pozisyonuna göre eklentiyi yap
                var newY = _startThumbY + e.TotalY;
                
                // Sınırları koru
                newY = Math.Max(0, Math.Min(maxThumbY, newY));

                ScrollThumbGrid.TranslationY = newY;

                // Calculate which item should be visible based on thumb position
                var scrollPercent = newY / maxThumbY;
                var targetIndex = (int)(scrollPercent * (_viewModel.Sureler.Count - 1));

                // Sadece index değiştiyse scroll yap (Aşırı çağrıyı önler)
                if (targetIndex != _lastMainTargetIndex && targetIndex >= 0 && targetIndex < _viewModel.Sureler.Count)
                {
                    _lastMainTargetIndex = targetIndex;
                    SureListView.ScrollTo(targetIndex, position: ScrollToPosition.Start, animate: false);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isPanning = false;
                break;
        }
    }

    private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (_isPanning) return; // Pan işlemi (sürükleme) sırasında konumun sıfırlanıp zıplamasını engelle
        
        var trackHeight = SureListView.Height;
        var thumbHeight = ScrollThumbGrid.Height;

        if (trackHeight > 0 && _viewModel.Sureler != null && _viewModel.Sureler.Count > 1)
        {
            // CollectionView ContentSize özelliğine sahip olmadığı için index tabanlı hesaplama kullanıyoruz
            double scrollPercent = (double)e.FirstVisibleItemIndex / (_viewModel.Sureler.Count - 1);
            
            // Sınırları koru
            scrollPercent = Math.Max(0, Math.Min(1, scrollPercent));

            var maxThumbY = trackHeight - thumbHeight;
            var thumbY = scrollPercent * maxThumbY;

            ScrollThumbGrid.TranslationY = thumbY;
        }
    }

    private void OnSiralamaTapped(object? sender, TappedEventArgs e)
    {
        // Hafif bir geri bildirim animasyonu (tıklama etkisi)
        if (sender is View view)
        {
            try { view.ScaleToAsync(0.95, 80); } catch { }
            try { view.ScaleToAsync(1.0, 80); } catch { }
        }
    }

    private void OnFabTapped(object? sender, TappedEventArgs e)
    {
        // FAB tıklama animasyonu
        if (sender is View view)
        {
            try { view.ScaleToAsync(0.90, 100); } catch { }
            try { view.ScaleToAsync(1.0, 100); } catch { }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _settings.SettingsChanged -= OnSettingsChanged;
    }
}
