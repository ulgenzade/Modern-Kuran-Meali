using SQLite;
namespace KuranMealApp.Models;

[Table("Mealler")]
[Preserve(AllMembers = true)]
public class Meal {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int AyetId { get; set; }
    [Ignore] public string CevirmenId { get; set; } = string.Empty;
    public string YazarAdi { get; set; } = string.Empty;
    public string MealMetni { get; set; } = string.Empty;
}