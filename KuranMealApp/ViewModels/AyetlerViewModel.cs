using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using KuranMealApp.Models;
using KuranMealApp.Services;

namespace KuranMealApp.ViewModels;

[QueryProperty(nameof(SureNo), "SureNo")]
public class AyetlerViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly IDatabaseService _db;
    private readonly ISettingsService _settings;

    private int _sureNo;
    private string _sureName = string.Empty;
    private bool _isBusy;
    private string _loadingMessage = "Yükleniyor...";
    private bool _useDyslexicFont;
    private bool _useHorizontalReadingMode;
    private double _fontSizeScale;
    private bool _showFilterSheet;
    private bool _showSettingsSheet;
    private bool _showSurePicker;
    private bool _isKlasikSira = true;
    private string _filterSearchText = string.Empty;
    private string _surePickerSearch = string.Empty;
    private string _jumpAyetNo = string.Empty;
    private bool _isTefsirPopupVisible;
    private bool _isNuzulPopupVisible;
    private AyetItem? _selectedAyetForPopup;
    private bool _isLoadingNext;
    private int _targetAyetNoToScroll = 0;

    private List<Sure> _allSureler = new();
    private List<TranslatorFilterItem> _allTranslators = new();
    private readonly HashSet<int> _loadedSureNos = new();

    // Scroll-to ayet: code-behind tarafından handle edilir
    public event Action<AyetItem, SureGroup>? ScrollToRequested;

    // ── Collections ──────────────────────────────────────────────────────
    public ObservableCollection<SureGroup> AyetGroups { get; } = new();
    public ObservableCollection<AyetItem> FlatAyetler { get; } = new();
    public ObservableCollection<TranslatorFilterItem> Translators { get; } = new();
    public ObservableCollection<Sure> SurePickerItems { get; } = new();

    // ── Properties ───────────────────────────────────────────────────────
    public int SureNo
    {
        get => _sureNo;
        set 
        { 
            if (SetProperty(ref _sureNo, value)) 
            {
                Preferences.Default.Set("LastReadSureNo", value);
                _ = LoadAyetlerAsync(); 
            }
        }
    }

    public string SureName
    {
        get => _sureName;
        set => SetProperty(ref _sureName, value);
    }

    private int _currentAyetNo = 1;
    public int CurrentAyetNo
    {
        get => _currentAyetNo;
        set 
        {
            if (SetProperty(ref _currentAyetNo, value))
            {
                Preferences.Default.Set("LastReadAyetNo", value);
            }
        }
    }

    private int _currentAyetId = 0;
    public int CurrentAyetId
    {
        get => _currentAyetId;
        set => SetProperty(ref _currentAyetId, value);
    }

    public ObservableCollection<AyetNumberItem> AvailableAyetNumbers { get; } = new();

    private bool _showAyetPicker;
    public bool ShowAyetPicker
    {
        get => _showAyetPicker;
        set => SetProperty(ref _showAyetPicker, value);
    }

    private int _selectedAyetNumber = 1;
    public int SelectedAyetNumber
    {
        get => _selectedAyetNumber;
        set => SetProperty(ref _selectedAyetNumber, value);
    }

    private Sure? _selectedSureForPicker;
    public Sure? SelectedSureForPicker
    {
        get => _selectedSureForPicker;
        set => SetProperty(ref _selectedSureForPicker, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public bool UseDyslexicFont
    {
        get => _useDyslexicFont;
        set
        {
            if (SetProperty(ref _useDyslexicFont, value))
            {
                _settings.UseDyslexicFont = value;
                OnPropertyChanged(nameof(FontFamilyName));
                UpdateItemsFontFamily();
            }
        }
    }

    public string FontFamilyName
    {
        get
        {
            if (UseDyslexicFont)
                return "OpenDyslexic";

            return _settings.SelectedFontFamily switch
            {
                "Inter" => "InterRegular",
                "Lora" => "LoraRegular",
                _ => "OpenSansRegular"
            };
        }
    }

    public void ReloadSettings()
    {
        _useDyslexicFont = _settings.UseDyslexicFont;
        _useHorizontalReadingMode = _settings.UseHorizontalReadingMode;
        OnPropertyChanged(nameof(UseDyslexicFont));
        OnPropertyChanged(nameof(UseHorizontalReadingMode));
        OnPropertyChanged(nameof(FontFamilyName));
        UpdateItemsFontFamily();
    }

    private void UpdateItemsFontFamily()
    {
        string font = FontFamilyName;
        foreach (var ayet in FlatAyetler)
        {
            ayet.FontFamilyName = font;
            if (ayet.Mealler != null)
            {
                foreach (var meal in ayet.Mealler)
                {
                    meal.FontFamilyName = font;
                }
            }
        }
    }

    public bool UseHorizontalReadingMode
    {
        get => _useHorizontalReadingMode;
        set
        {
            if (SetProperty(ref _useHorizontalReadingMode, value))
            {
                _settings.UseHorizontalReadingMode = value;
                OnPropertyChanged(nameof(IsVerticalMode));
                OnPropertyChanged(nameof(IsHorizontalMode));
            }
        }
    }

    public bool IsDarkMode
    {
        get => _settings.IsDarkMode;
        set
        {
            if (_settings.IsDarkMode != value)
            {
                _settings.IsDarkMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DarkModeButtonText));
                Application.Current.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
            }
        }
    }

    public string DarkModeButtonText => IsDarkMode ? "☀️ Aydınlık Mod" : "🌙 Karanlık Mod";

    public bool IsVerticalMode => !UseHorizontalReadingMode;
    public bool IsHorizontalMode => UseHorizontalReadingMode;

    public double FontSizeScale
    {
        get => _fontSizeScale;
        set
        {
            if (SetProperty(ref _fontSizeScale, value))
            {
                _settings.FontSizeScale = value;
                OnPropertyChanged(nameof(ArabicFontSize));
                OnPropertyChanged(nameof(TranscriptFontSize));
                OnPropertyChanged(nameof(MealFontSize));
                OnPropertyChanged(nameof(AciklamaFontSize));
                OnPropertyChanged(nameof(FontSizePercent));
            }
        }
    }

    public double ArabicFontSize => 26 * FontSizeScale;
    public double TranscriptFontSize => 14 * FontSizeScale;
    public double MealFontSize => 15 * FontSizeScale;
    public double AciklamaFontSize => 13 * FontSizeScale;
    public string FontSizePercent => $"{(int)(FontSizeScale * 100)}%";

    public bool ShowFilterSheet
    {
        get => _showFilterSheet;
        set => SetProperty(ref _showFilterSheet, value);
    }

    public bool ShowSettingsSheet
    {
        get => _showSettingsSheet;
        set => SetProperty(ref _showSettingsSheet, value);
    }

    public bool ShowSurePicker
    {
        get => _showSurePicker;
        set => SetProperty(ref _showSurePicker, value);
    }

    public bool IsKlasikSira
    {
        get => _isKlasikSira;
        set => SetProperty(ref _isKlasikSira, value);
    }

    public string FilterSearchText
    {
        get => _filterSearchText;
        set { if (SetProperty(ref _filterSearchText, value)) FilterTranslators(); }
    }

    public string SurePickerSearch
    {
        get => _surePickerSearch;
        set { if (SetProperty(ref _surePickerSearch, value)) FilterSurePicker(); }
    }

    public string JumpAyetNo
    {
        get => _jumpAyetNo;
        set => SetProperty(ref _jumpAyetNo, value);
    }

    public bool IsTefsirPopupVisible
    {
        get => _isTefsirPopupVisible;
        set => SetProperty(ref _isTefsirPopupVisible, value);
    }

    public bool IsNuzulPopupVisible
    {
        get => _isNuzulPopupVisible;
        set => SetProperty(ref _isNuzulPopupVisible, value);
    }

    public AyetItem? SelectedAyetForPopup
    {
        get => _selectedAyetForPopup;
        set => SetProperty(ref _selectedAyetForPopup, value);
    }

    // ── Commands ─────────────────────────────────────────────────────────
    public ICommand ToggleFilterSheetCommand { get; }
    public ICommand ApplyFilterCommand { get; }
    public ICommand SelectAllTranslatorsCommand { get; }
    public ICommand ClearTranslatorsCommand { get; }
    public ICommand ToggleAciklamaCommand { get; }
    public ICommand ToggleNuzulCommand { get; }
    public ICommand ShowTefsirPopupCommand { get; }
    public ICommand ShowNuzulPopupCommand { get; }
    public ICommand ClosePopupsCommand { get; }
    public ICommand LoadNextSureCommand { get; }
    public ICommand NavigatePrevSureCommand { get; }
    public ICommand NavigateNextSureCommand { get; }
    public ICommand ToggleSettingsSheetCommand { get; }
    public ICommand ToggleSurePickerCommand { get; }
    public ICommand JumpToSureCommand { get; }
    public ICommand ConfirmSureCommand { get; }
    public ICommand ToggleAyetPickerCommand { get; }
    public ICommand JumpToAyetCommand { get; }
    public ICommand ConfirmAyetCommand { get; }
    public ICommand ToggleDarkModeCommand { get; }
    public ICommand IncreaseFontSizeCommand { get; }
    public ICommand DecreaseFontSizeCommand { get; }
    public ICommand SetReadingModeCommand { get; }
    public ICommand SetSiralamaCommand { get; }
    public ICommand ToggleTranslatorSelectionCommand { get; }

    public AyetlerViewModel(IDatabaseService db, ISettingsService settings)
    {
        _db = db;
        _settings = settings;

        _useDyslexicFont = _settings.UseDyslexicFont;
        _useHorizontalReadingMode = _settings.UseHorizontalReadingMode;
        _fontSizeScale = _settings.FontSizeScale;

        SetReadingModeCommand = new Command<string>(mode =>
        {
            if (mode == "Yatay") UseHorizontalReadingMode = true;
            else if (mode == "Dikey") UseHorizontalReadingMode = false;
        });

        SetSiralamaCommand = new Command<string>(async siralama =>
        {
            if (siralama == "Klasik") IsKlasikSira = true;
            else if (siralama == "Inis") IsKlasikSira = false;
            await LoadAyetlerAsync();
        });

        ToggleFilterSheetCommand = new Command(async () => await ToggleFilterSheet());
        ApplyFilterCommand = new Command(async () => await ApplyFilter());
        SelectAllTranslatorsCommand = new Command(() => { foreach (var t in _allTranslators) t.IsSelected = true; FilterTranslators(); });
        ClearTranslatorsCommand = new Command(() => { foreach (var t in _allTranslators) t.IsSelected = false; FilterTranslators(); });
        ToggleAciklamaCommand = new Command<MealWithAciklama>(m => { if (m != null) m.IsAciklamaExpanded = !m.IsAciklamaExpanded; });
        ToggleTranslatorSelectionCommand = new Command<TranslatorFilterItem>(item => { if (item != null) item.IsSelected = !item.IsSelected; });
        ToggleNuzulCommand = new Command<AyetItem>(a => { if (a != null) a.IsNuzulExpanded = !a.IsNuzulExpanded; });
        
        ShowTefsirPopupCommand = new Command<AyetItem>(a => { 
            SelectedAyetForPopup = a; 
            IsTefsirPopupVisible = true; 
        });
        
        ShowNuzulPopupCommand = new Command<AyetItem>(a => { 
            SelectedAyetForPopup = a; 
            IsNuzulPopupVisible = true; 
        });
        
        ClosePopupsCommand = new Command(() => { 
            IsTefsirPopupVisible = false; 
            IsNuzulPopupVisible = false; 
            ShowAyetPicker = false;
            SelectedAyetForPopup = null; 
        });

        LoadNextSureCommand = new Command(() => { if (SureNo < 114) SureNo++; });
        NavigatePrevSureCommand = new Command(() => { if (SureNo > 1) SureNo--; });
        NavigateNextSureCommand = new Command(() => { if (SureNo < 114) SureNo++; });
        ToggleDarkModeCommand = new Command(() => IsDarkMode = !IsDarkMode);
        ToggleSettingsSheetCommand = new Command(() => ShowSettingsSheet = !ShowSettingsSheet);
        ToggleSurePickerCommand = new Command(async () =>
        {
            if (!ShowSurePicker)
            {
                SurePickerSearch = string.Empty;
                FilterSurePicker();
                SelectedSureForPicker = _allSureler?.FirstOrDefault(s => s.SureNo == SureNo);
                
                await Task.Delay(50);
                ShowSurePicker = true;
            }
            else
            {
                ShowSurePicker = false;
            }
        });
        
        JumpToSureCommand = new Command<Sure>(s =>
        {
            if (s != null)
            {
                SelectedSureForPicker = s;
                ShowSurePicker = false;
                SureNo = s.SureNo;
            }
        });

        ConfirmSureCommand = new Command(async () =>
        {
            if (SelectedSureForPicker != null)
            {
                ShowSurePicker = false;
                SureNo = SelectedSureForPicker.SureNo; // triggers LoadAyetlerAsync
            }
        });

        ToggleAyetPickerCommand = new Command(() =>
        {
            if (!ShowAyetPicker)
            {
                SelectedAyetNumber = CurrentAyetNo;
                foreach (var num in AvailableAyetNumbers)
                {
                    num.IsSelected = (num.Number == CurrentAyetNo);
                }
            }
            ShowAyetPicker = !ShowAyetPicker;
        });

        JumpToAyetCommand = new Command<AyetNumberItem>(item =>
        {
            if (item != null)
            {
                SelectedAyetNumber = item.Number;
                foreach (var num in AvailableAyetNumbers)
                {
                    num.IsSelected = (num.Number == item.Number);
                }
            }
        });

        ConfirmAyetCommand = new Command(() =>
        {
            ShowAyetPicker = false;
            var firstGroup = AyetGroups.FirstOrDefault();
            if (firstGroup != null)
            {
                var item = firstGroup.FirstOrDefault(a => a.AyetNo == SelectedAyetNumber);
                if (item != null)
                {
                    ScrollToRequested?.Invoke(item, firstGroup);
                }
            }
        });

        IncreaseFontSizeCommand = new Command(() => { if (FontSizeScale < 2.0) FontSizeScale = Math.Round(FontSizeScale + 0.1, 1); });
        DecreaseFontSizeCommand = new Command(() => { if (FontSizeScale > 0.7) FontSizeScale = Math.Round(FontSizeScale - 0.1, 1); });

        _settings.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
    {
        if (e.SettingName == nameof(ISettingsService.SelectedFontFamily))
        {
            if (!UseDyslexicFont)
            {
                OnPropertyChanged(nameof(FontFamilyName));
                UpdateItemsFontFamily();
            }
        }
        else if (e.SettingName == nameof(ISettingsService.UseDyslexicFont))
        {
            if (UseDyslexicFont != (bool)e.NewValue)
            {
                UseDyslexicFont = (bool)e.NewValue;
            }
        }
        else if (e.SettingName == nameof(ISettingsService.FontSizeScale))
        {
            if (Math.Abs(FontSizeScale - (double)e.NewValue) > 0.001)
            {
                FontSizeScale = (double)e.NewValue;
            }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("SureNo", out var v) && int.TryParse(v?.ToString(), out int no))
        {
            if (query.TryGetValue("AyetNo", out var av) && int.TryParse(av?.ToString(), out int ayetNo))
            {
                _targetAyetNoToScroll = ayetNo;
            }
            else if (query.TryGetValue("JumpAyetNo", out var jav) && int.TryParse(jav?.ToString(), out int jumpAyetNo))
            {
                _targetAyetNoToScroll = jumpAyetNo;
            }
            SureNo = no;
        }
    }

    // ── Loading ───────────────────────────────────────────────────────────

    private async Task LoadAyetlerAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        LoadingMessage = "Sure yükleniyor...";

        try
        {
            await _db.InitializeAsync();
            AyetGroups.Clear();
            _loadedSureNos.Clear();

            _allSureler = await _db.GetSurelerAsync();
            var sure = _allSureler.FirstOrDefault(s => s.SureNo == SureNo);
            if (sure != null) SureName = sure.SureAdi;

            var allAuthors = await _db.GetAvailableTranslatorsAsync();
            var selected = _settings.SelectedTranslators;
            _allTranslators = allAuthors.Select(n => new TranslatorFilterItem
            {
                Name = n,
                IsSelected = selected.Contains(n)
            }).ToList();
            FilterTranslators();
            FilterSurePicker();

            FlatAyetler.Clear();
            _loadedSureNos.Clear();

            await AppendSureAsync(SureNo);
            
            // Ayet numaralarını Picker için dolduralım
            AvailableAyetNumbers.Clear();
            if (sure != null)
            {
                for (int i = 1; i <= sure.AyetSayisi; i++)
                {
                    AvailableAyetNumbers.Add(new AyetNumberItem { Number = i, IsSelected = (i == SelectedAyetNumber) });
                }
            }
            
            if (_targetAyetNoToScroll > 0)
            {
                var firstGroup = AyetGroups.FirstOrDefault();
                if (firstGroup != null)
                {
                    var item = firstGroup.FirstOrDefault(a => a.AyetNo == _targetAyetNoToScroll);
                    if (item != null)
                    {
                        ScrollToRequested?.Invoke(item, firstGroup);
                        CurrentAyetId = item.AyetId;
                    }
                }
                _targetAyetNoToScroll = 0; // Reset after scroll
            }
            else
            {
                // Scroll to top by default on new sure load
                var firstGroup = AyetGroups.FirstOrDefault();
                var firstItem = firstGroup?.FirstOrDefault();
                if (firstItem != null)
                {
                    ScrollToRequested?.Invoke(firstItem, firstGroup!);
                    CurrentAyetNo = firstItem.AyetNo;
                    CurrentAyetId = firstItem.AyetId;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HATA (LoadAyetlerAsync): {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadNextSure()
    {
        if (SureNo < 114) SureNo++;
    }

    private async Task AppendSureAsync(int sureNo)
    {
        if (_loadedSureNos.Contains(sureNo)) return;

        var sure = _allSureler.FirstOrDefault(s => s.SureNo == sureNo);
        if (sure == null)
        {
            sure = await _db.GetSureByNoAsync(sureNo);
            if (sure == null) return;
        }

        var selected = _settings.SelectedTranslators;

        // Veritabanı sorgularını arka plana al
        var items = await Task.Run(async () =>
        {
            var rawAyets = await _db.GetAyetlerAsync(sure.Id);
            var result = new List<AyetItem>();
            foreach (var rawAyet in rawAyets)
            {
                var item = new AyetItem(rawAyet);
                item.Mealler = await _db.GetMeallerForAyetAsync(rawAyet.Id, selected);
                InitializeFontForAyet(item);
                var nuzulRows = await _db.GetNuzulSebepleriForAyetAsync(rawAyet.Id);
                if (nuzulRows.Any())
                    item.NuzulSebebiText = string.Join("\n\n", nuzulRows.Select(n => StripDevami(n.Aciklama)));
                result.Add(item);
            }
            return result;
        });

        var group = new SureGroup(sure.SureAdi, sureNo, sure.AyetSayisi, items);
        
        MainThread.BeginInvokeOnMainThread(() => 
        {
            AyetGroups.Add(group);
            foreach (var item in items) FlatAyetler.Add(item);
            _loadedSureNos.Add(sureNo);
        });
    }

    private void InitializeFontForAyet(AyetItem item)
    {
        string font = FontFamilyName;
        item.FontFamilyName = font;
        if (item.Mealler != null)
        {
            foreach (var m in item.Mealler)
            {
                m.FontFamilyName = font;
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static string StripDevami(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // "Devamı.." "Devamı..." gibi sondaki truncation kalıplarını sil
        return Regex.Replace(text, @"\s*Devam[ıi]\.+\s*$", "", RegexOptions.IgnoreCase).Trim();
    }

    private async Task ToggleFilterSheet()
    {
        if (!ShowFilterSheet)
        {
            FilterSearchText = string.Empty;
            var sel = _settings.SelectedTranslators;
            foreach (var t in _allTranslators) t.IsSelected = sel.Contains(t.Name);
            FilterTranslators();
            
            await Task.Delay(50);
            ShowFilterSheet = true;
        }
        else
        {
            ShowFilterSheet = false;
        }
    }

    private async Task ApplyFilter()
    {
        var selected = _allTranslators.Where(t => t.IsSelected).Select(t => t.Name).ToList();
        if (!selected.Any())
        {
            var fallback = _allTranslators.FirstOrDefault(t => t.Name.Contains("Diyanet İşleri"));
            if (fallback != null) { fallback.IsSelected = true; selected.Add(fallback.Name); }
            else if (_allTranslators.Any()) { _allTranslators.First().IsSelected = true; selected.Add(_allTranslators.First().Name); }
        }

        _settings.SelectedTranslators = selected;
        ShowFilterSheet = false;
        IsBusy = true;
        LoadingMessage = "Filtre uygulanıyor...";
        try
        {
            // Kaydırma konumunu sakla
            int targetAyetId = CurrentAyetId;
            int targetAyetNo = CurrentAyetNo;

            // Tüm gruplardaki mealleri yeniden yükle
            var loadedNos = _loadedSureNos.OrderBy(x => x).ToList();
            AyetGroups.Clear();
            FlatAyetler.Clear();
            _loadedSureNos.Clear();
            foreach (var no in loadedNos)
                await AppendSureAsync(no);

            // Kaydırma konumunu geri yükle
            AyetItem? targetItem = null;
            SureGroup? targetGroup = null;

            foreach (var g in AyetGroups)
            {
                targetItem = g.FirstOrDefault(a => a.AyetId == targetAyetId);
                if (targetItem != null)
                {
                    targetGroup = g;
                    break;
                }
            }

            if (targetItem == null && targetAyetNo > 0)
            {
                var firstGroup = AyetGroups.FirstOrDefault();
                if (firstGroup != null)
                {
                    targetItem = firstGroup.FirstOrDefault(a => a.AyetNo == targetAyetNo);
                    targetGroup = firstGroup;
                }
            }

            if (targetItem != null && targetGroup != null)
            {
                await Task.Delay(100); // Layout'un tamamlanması için kısa bir süre bekle
                ScrollToRequested?.Invoke(targetItem, targetGroup);
                CurrentAyetNo = targetItem.AyetNo;
                CurrentAyetId = targetItem.AyetId;
            }
        }
        finally { IsBusy = false; }
    }

    private void FilterTranslators()
    {
        Translators.Clear();
        var query = _allTranslators.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(FilterSearchText))
            query = query.Where(t => t.Name.ToLowerInvariant().Contains(FilterSearchText.ToLowerInvariant().Trim()));
        foreach (var t in query) Translators.Add(t);
    }

    private void FilterSurePicker()
    {
        SurePickerItems.Clear();
        var query = _allSureler.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SurePickerSearch))
        {
            var s = SurePickerSearch.Trim().ToLowerInvariant();
            query = query.Where(sure =>
                sure.SureAdi.ToLowerInvariant().Contains(s) ||
                sure.SureNo.ToString().StartsWith(s));
        }
        foreach (var sure in query) SurePickerItems.Add(sure);
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(name);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    #endregion
}

public class TranslatorFilterItem : INotifyPropertyChanged
{
    private bool _isSelected;
    public string Name { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class AyetNumberItem : INotifyPropertyChanged
{
    private bool _isSelected;
    public int Number { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
