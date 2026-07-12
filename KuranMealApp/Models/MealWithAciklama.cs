using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace KuranMealApp.Models;

[Preserve(AllMembers = true)]
public class MealWithAciklama : INotifyPropertyChanged {
    public int MealId { get; set; }
    public string YazarAdi { get; set; } = string.Empty;
    public string MealMetni { get; set; } = string.Empty;
    public string AciklamaMetni { get; set; } = string.Empty;
    
    public bool HasAciklama => !string.IsNullOrEmpty(AciklamaMetni);
    public string DisplayAciklamaMetni => HasAciklama ? AciklamaMetni : $"{YazarAdi}'in bu ayet hakkında tefsiri yoktur.";
    public string DisplayYazarAdi => YazarAdi + (HasAciklama ? " 📖" : "") + ": ";
    
    private bool _isAciklamaExpanded;
    public bool IsAciklamaExpanded { get => _isAciklamaExpanded; set { _isAciklamaExpanded = value; OnPropertyChanged(); } }
    
    private string _fontFamilyName = "OpenSansRegular";
    public string FontFamilyName { get => _fontFamilyName; set { _fontFamilyName = value; OnPropertyChanged(); } }
    
    public MealWithAciklama(Meal meal, string aciklama) {
        MealId = meal.Id;
        YazarAdi = meal.YazarAdi;
        MealMetni = meal.MealMetni;
        AciklamaMetni = aciklama ?? string.Empty;
        _isAciklamaExpanded = HasAciklama && AciklamaMetni.Length <= 250;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
    }
}