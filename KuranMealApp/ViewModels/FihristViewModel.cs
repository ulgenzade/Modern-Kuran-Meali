using System.Collections.ObjectModel;
using System.Windows.Input;
using KuranMealApp.Models;
using KuranMealApp.Services;

namespace KuranMealApp.ViewModels;

public class FihristKonu
{
    public string Baslik { get; set; } = string.Empty;
    public string İkon { get; set; } = string.Empty;
    public string AramaTerimi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
}


public class FihristViewModel : BindableObject
{
    public ObservableCollection<FihristKonu> Konular { get; } = new();

    public ICommand KonuSecildiCommand { get; }

    public FihristViewModel()
    {
        Konular.Add(new FihristKonu { İkon = "⚖️", Baslik = "Adalet", AramaTerimi = "Adalet", Aciklama = "Kur'an'da adalet, hak ve hukuk" });
        Konular.Add(new FihristKonu { İkon = "🤲", Baslik = "Dua", AramaTerimi = "Dua", Aciklama = "Yakarış ve tövbe ayetleri" });
        Konular.Add(new FihristKonu { İkon = "🕌", Baslik = "Namaz", AramaTerimi = "Namaz", Aciklama = "İbadet ve namaz" });
        Konular.Add(new FihristKonu { İkon = "🌙", Baslik = "Oruç", AramaTerimi = "Oruç", Aciklama = "Ramazan ve oruç ibadeti" });
        Konular.Add(new FihristKonu { İkon = "💖", Baslik = "Sabır", AramaTerimi = "Sabır", Aciklama = "Zorluklar karşısında sabır" });
        Konular.Add(new FihristKonu { İkon = "🤝", Baslik = "Zekat & Sadaka", AramaTerimi = "Zekat", Aciklama = "Yardımlaşma ve infak" });
        Konular.Add(new FihristKonu { İkon = "🕊️", Baslik = "Barış", AramaTerimi = "Barış", Aciklama = "İslam'da barış ve esenlik" });
        Konular.Add(new FihristKonu { İkon = "👨‍👩‍👧", Baslik = "Aile", AramaTerimi = "Aile", Aciklama = "Aile hayatı, anne ve baba" });
        Konular.Add(new FihristKonu { İkon = "✨", Baslik = "İman", AramaTerimi = "İman", Aciklama = "Tevhid ve inanç esasları" });
        Konular.Add(new FihristKonu { İkon = "🧠", Baslik = "Akıl", AramaTerimi = "Akıl", Aciklama = "Düşünme ve ibret alma" });
        
        KonuSecildiCommand = new Command<FihristKonu>(async (konu) =>
        {
            if (konu == null) return;
            // Arama sayfasına seçilen terimle git (Tab bar üzerinden Arama sekmesine geçerken parametre yollama)
            // Ya da AramaViewModel singleton olmadığı için shell navigasyonuyla state aktarmak zor olabilir.
            // Fakat basitçe "Arama" sayfasına yönlendirip orada aratabiliriz.
            
            // Eğer Navigation stack kullanıyorsak Arama sekmesini seçtirtip içine yazdırabiliriz.
            // Ya da yeni bir sonuç sayfası açarız. En kolayı Fihrist'e özel bir sonuç veya Arama sekmesine aktarmak.
            // Arama sekmesi Shell'de "arama" route'una sahip.
            await Shell.Current.GoToAsync($"///AramaPage?query={konu.AramaTerimi}");
        });
    }
}
