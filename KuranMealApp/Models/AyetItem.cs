using System.ComponentModel;
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
    
    public bool HasNuzulSebebi => !string.IsNullOrEmpty(NuzulSebebiText);
    public bool HasTefsir => Mealler.Any(m => m.HasAciklama);
    public bool HasAnyAction => HasNuzulSebebi || HasTefsir;
    
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
}