using SQLite;
namespace KuranMealApp.Models;

[Table("NuzulSebepleri")]
[Preserve(AllMembers = true)]
public class NuzulSebebi {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Ignore] public int SureId { get; set; }
    [Indexed] public int AyetId { get; set; }
    [Ignore] public int? AyetNo { get; set; }
    [Ignore] public string SebepMetni { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
}