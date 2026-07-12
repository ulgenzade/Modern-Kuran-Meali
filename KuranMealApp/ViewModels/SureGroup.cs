using KuranMealApp.Models;
using System.Collections.ObjectModel;

namespace KuranMealApp.ViewModels;

/// <summary>
/// Grouped CollectionView için sure bazlı gruplama.
/// </summary>
public class SureGroup : ObservableCollection<AyetItem>
{
    public string SureAdi { get; }
    public int SureNo { get; }
    public int AyetSayisi { get; }
    public string GroupHeader => $"{SureNo}. {SureAdi}  •  {AyetSayisi} ayet";

    public SureGroup(string sureAdi, int sureNo, int ayetSayisi, IEnumerable<AyetItem> items)
        : base(items)
    {
        SureAdi = sureAdi;
        SureNo = sureNo;
        AyetSayisi = ayetSayisi;
    }
}
