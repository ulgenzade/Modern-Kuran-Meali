import os

models = {
'AramaSonucu.cs': '''namespace KuranMealApp.Models;

[Preserve(AllMembers = true)]
public class AramaSonucu {
    public int AyetId { get; set; }
    public string SureAdi { get; set; } = string.Empty;
    public int SureNo { get; set; }
    public int AyetNo { get; set; }
    public string MealMetni { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
}''',
'Ayet.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("Ayetler")]
[Preserve(AllMembers = true)]
public class Ayet {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int SureId { get; set; }
    public int AyetNo { get; set; }
    public string ArapcaMetin { get; set; } = string.Empty;
    public string Transkript { get; set; } = string.Empty;
}''',
'AyetItem.cs': '''using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
namespace KuranMealApp.Models;

[Preserve(AllMembers = true)]
public class AyetItem : INotifyPropertyChanged {
    public int AyetId { get; set; }
    public int SureId { get; set; }
    public int AyetNo { get; set; }
    public string ArapcaMetin { get; set; } = string.Empty;
    public string Transkript { get; set; } = string.Empty;
    public string NuzulSebebiText { get; set; } = string.Empty;
    public List<MealWithAciklama> Mealler { get; set; } = new();
    
    private bool _isNuzulExpanded;
    public bool IsNuzulExpanded { get => _isNuzulExpanded; set { _isNuzulExpanded = value; OnPropertyChanged(); } }
    
    public AyetItem(Ayet ayet) {
        AyetId = ayet.Id;
        SureId = ayet.SureId;
        AyetNo = ayet.AyetNo;
        ArapcaMetin = ayet.ArapcaMetin;
        Transkript = ayet.Transkript;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
    }
}''',
'Meal.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("Mealler")]
[Preserve(AllMembers = true)]
public class Meal {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int AyetId { get; set; }
    public int CevirmenId { get; set; }
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
    
    public MealWithAciklama(Meal meal, MealAciklama? aciklama) {
        MealId = meal.Id;
        YazarAdi = meal.YazarAdi;
        MealMetni = meal.MealMetni;
        AciklamaMetni = aciklama?.AciklamaMetni ?? string.Empty;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") { 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
    }
}''',
'NuzulSebebi.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("NuzulSebepleri")]
[Preserve(AllMembers = true)]
public class NuzulSebebi {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int SureId { get; set; }
    [Indexed] public int AyetId { get; set; }
    public int? AyetNo { get; set; }
    public string SebepMetni { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
}''',
'Sure.cs': '''using SQLite;
namespace KuranMealApp.Models;

[Table("Sureler")]
[Preserve(AllMembers = true)]
public class Sure {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed(Unique = true)] public int SureNo { get; set; }
    [Column("SureAdi")] public string SureAdi { get; set; } = string.Empty;
    public int AyetSayisi { get; set; }
    public int InisSirasi { get; set; }
    public string Tip { get; set; } = "Mekkî";
}'''
}

for name, content in models.items():
    with open(f'KuranMealApp/Models/{name}', 'w', encoding='utf-8') as f:
        f.write(content)
