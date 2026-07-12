import os

models = {
'Meal.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("Mealler")]
[Preserve(AllMembers = true)]
public class Meal {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int AyetId { get; set; }
    public string CevirmenId { get; set; } = string.Empty;
    public string YazarAdi { get; set; } = string.Empty;
    public string MealMetni { get; set; } = string.Empty;
}''',
'MealAciklama.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("MealAciklamalari")]
[Preserve(AllMembers = true)]
public class MealAciklama {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int AyetId { get; set; }
    public string CevirmenId { get; set; } = string.Empty;
    public string YazarAdi { get; set; } = string.Empty;
    public string AciklamaMetni { get; set; } = string.Empty;
}''',
'MealWithAciklama.cs': '''using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace KuranMealApp.Models;

[Preserve(AllMembers = true)]
public class MealWithAciklama : INotifyPropertyChanged {
    public int MealId { get; set; }
    public string YazarAdi { get; set; } = string.Empty;
    public string MealMetni { get; set; } = string.Empty;
    public string AciklamaMetni { get; set; } = string.Empty;
    
    private bool _isAciklamaExpanded;
    public bool IsAciklamaExpanded { get => _isAciklamaExpanded; set { _isAciklamaExpanded = value; OnPropertyChanged(); } }
    
    public MealWithAciklama(Meal meal, string aciklama) {
        MealId = meal.Id;
        YazarAdi = meal.YazarAdi;
        MealMetni = meal.MealMetni;
        AciklamaMetni = aciklama ?? string.Empty;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
    }
}'''
}

for name, content in models.items():
    with open(f'KuranMealApp/Models/{name}', 'w', encoding='utf-8') as f:
        f.write(content)
