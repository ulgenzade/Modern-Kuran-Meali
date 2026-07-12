using SQLite;
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
}