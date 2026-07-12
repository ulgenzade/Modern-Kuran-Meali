using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KuranMealApp.Models;
using KuranMealApp.Services;

namespace KuranMealApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private List<Sure> _allSureler = new();
    private bool _sortByChronology;
    private string _searchText = string.Empty;
    private bool _isBusy;
    private bool _isKlasikSira = true;

    public ObservableCollection<Sure> Sureler { get; } = new();

    public bool IsKlasikSira
    {
        get => _isKlasikSira;
        set => SetProperty(ref _isKlasikSira, value);
    }

    public bool SortByChronology
    {
        get => _sortByChronology;
        set
        {
            if (SetProperty(ref _sortByChronology, value))
            {
                _ = LoadSurelerAsync();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterSureler();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand LoadSurelerCommand { get; }
    public ICommand SetSiralamaCommand { get; }
    public ICommand ContinueReadingCommand { get; }

    public MainViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        LoadSurelerCommand = new Command(async () => await LoadSurelerAsync());
        SetSiralamaCommand = new Command<string>(SetSiralama);
        ContinueReadingCommand = new Command(async () => await ContinueReadingAsync());
    }

    private void SetSiralama(string mode)
    {
        IsKlasikSira = mode == "Klasik";
        SortByChronology = !IsKlasikSira;
    }

    private async Task ContinueReadingAsync()
    {
        // Get last read from preferences, default to Fatiha 1
        int lastSure = Preferences.Default.Get("LastReadSureNo", 1);
        int lastAyet = Preferences.Default.Get("LastReadAyetNo", 1);
        
        var sure = _allSureler.FirstOrDefault(s => s.SureNo == lastSure);
        if (sure != null)
        {
            await Shell.Current.GoToAsync($"AyetlerPage?SureNo={sure.SureNo}&JumpAyetNo={lastAyet}");
        }
    }

    public async Task LoadSurelerAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            await _databaseService.InitializeAsync();
            _allSureler = await _databaseService.GetSurelerAsync(SortByChronology);
            FilterSureler();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Error loading surahs: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterSureler()
    {
        Sureler.Clear();
        var query = _allSureler.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant().Trim();
            query = query.Where(s => 
                s.SureAdi.ToLowerInvariant().Contains(search) || 
                s.SureNo.ToString() == search
            );
        }

        foreach (var sure in query)
        {
            Sureler.Add(sure);
        }
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
