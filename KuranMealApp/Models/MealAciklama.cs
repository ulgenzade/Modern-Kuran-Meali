using SQLite;
namespace KuranMealApp.Models;

[Table("MealAciklamalari")]
[Preserve(AllMembers = true)]
public class MealAciklama {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int AyetId { get; set; }
    public string CevirmenId { get; set; } = string.Empty;
    public string YazarAdi { get; set; } = string.Empty;
    public string AciklamaMetni { get; set; } = string.Empty;
}