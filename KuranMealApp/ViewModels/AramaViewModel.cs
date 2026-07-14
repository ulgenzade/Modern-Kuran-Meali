using System.Collections.ObjectModel;
using System.Windows.Input;
using KuranMealApp.Models;
using KuranMealApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KuranMealApp.ViewModels;

public class AramaViewModel : BindableObject, IQueryAttributable
{
    private readonly IDatabaseService _db;
    private readonly ISettingsService _settings;

    private string _searchQuery = string.Empty;
    private bool _isSearching;
    private string _searchMessage = "Arama yapmak için yukarıya bir kelime yazın.";

    private string _selectedTranslator = "Diyanet İşleri Meali (Yeni)";
    public ObservableCollection<TranslatorFilterItem> AvailableTranslators { get; } = new();

    private bool _isTranslatorPickerVisible;
    public bool IsTranslatorPickerVisible
    {
        get => _isTranslatorPickerVisible;
        set { _isTranslatorPickerVisible = value; OnPropertyChanged(); }
    }

    public ObservableCollection<AramaSonucu> SearchResults { get; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("query", out var v) && v != null)
        {
            SearchQuery = v.ToString() ?? string.Empty;
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            OnPropertyChanged();
            _ = ExecuteSearchAsync();
        }
    }

    public string SelectedTranslator
    {
        get => _selectedTranslator;
        set
        {
            if (_selectedTranslator != value && value != null)
            {
                _selectedTranslator = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedTranslatorDisplay));
                _ = ExecuteSearchAsync(); // Yeniden ara
            }
        }
    }

    public string SelectedTranslatorDisplay => string.IsNullOrEmpty(_selectedTranslator) ? "Tefsir Seçin" : _selectedTranslator;

    public bool IsSearching
    {
        get => _isSearching;
        set { _isSearching = value; OnPropertyChanged(); }
    }

    public string SearchMessage
    {
        get => _searchMessage;
        set { _searchMessage = value; OnPropertyChanged(); }
    }

    public ICommand ItemTappedCommand { get; }
    public ICommand ToggleTranslatorPickerCommand { get; }
    public ICommand ToggleTranslatorSelectionCommand { get; }
    public ICommand ConfirmTranslatorCommand { get; }
    public ICommand ClearTranslatorCommand { get; }

    public AramaViewModel(IDatabaseService db, ISettingsService settings)
    {
        _db = db;
        _settings = settings;

        _ = LoadTranslatorsAsync();

        ItemTappedCommand = new Command<AramaSonucu>(async (item) =>
        {
            if (item == null) return;
            await Shell.Current.GoToAsync($"{nameof(KuranMealApp.Views.AyetlerPage)}?SureNo={item.SureNo}&AyetNo={item.AyetNo}");
        });

        ToggleTranslatorPickerCommand = new Command(() => IsTranslatorPickerVisible = !IsTranslatorPickerVisible);

        ToggleTranslatorSelectionCommand = new Command<TranslatorFilterItem>(item =>
        {
            if (item == null) return;
            item.IsSelected = !item.IsSelected;
        });

        ConfirmTranslatorCommand = new Command(() =>
        {
            var selected = AvailableTranslators.FirstOrDefault(t => t.IsSelected);
            if (selected != null)
            {
                SelectedTranslator = selected.Name;
            }
            IsTranslatorPickerVisible = false;
        });

        ClearTranslatorCommand = new Command(() =>
        {
            foreach (var t in AvailableTranslators) t.IsSelected = false;
            SelectedTranslator = string.Empty;
            IsTranslatorPickerVisible = false;
        });
    }

    private async Task LoadTranslatorsAsync()
    {
        try
        {
            var allAuthors = await _db.GetAvailableTranslatorsAsync();
            
            MainThread.BeginInvokeOnMainThread(() => 
            {
                var list = new List<string> { "Diyanet İşleri Meali (Yeni)" };
                foreach (var author in allAuthors.Where(a => a != "Diyanet İşleri Meali (Yeni)").OrderBy(a => a))
                {
                    list.Add(author);
                }

                AvailableTranslators.Clear();
                foreach (var item in list)
                {
                    AvailableTranslators.Add(new TranslatorFilterItem { Name = item, IsSelected = item == SelectedTranslator });
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading translators: {ex}");
        }
    }

    private CancellationTokenSource? _debounceCts;

    private async Task ExecuteSearchAsync()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        try
        {
            await Task.Delay(500, token); // Debounce 500ms

            if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 3)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SearchResults.Clear();
                    SearchMessage = "Arama yapmak için yukarıya en az 3 harf yazın.";
                    IsSearching = false;
                });
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsSearching = true;
                SearchMessage = "Aranıyor...";
                SearchResults.Clear();
            });

            var results = await _db.SearchAyetlerAsync(SearchQuery, new List<string> { SelectedTranslator });

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (token.IsCancellationRequested) return;

                foreach (var res in results)
                {
                    res.SearchQuery = SearchQuery;
                    SearchResults.Add(res);
                }

                if (!results.Any())
                {
                    SearchMessage = "Sonuç bulunamadı.";
                }
                else
                {
                    SearchMessage = $"{results.Count} sonuç bulundu.";
                }
                IsSearching = false;
            });
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Arama Hatası]: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsSearching = false;
                SearchMessage = "Arama sırasında bir hata oluştu.";
            });
        }
    }
}

