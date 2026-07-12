using SQLite;
namespace KuranMealApp.Models;

[Table("Ayetler")]
[Preserve(AllMembers = true)]
public class Ayet {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int SureId { get; set; }
    public int AyetNo { get; set; }
    public string ArapcaMetin { get; set; } = string.Empty;
    public string Transkript { get; set; } = string.Empty;
}