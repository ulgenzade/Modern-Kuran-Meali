# Modern Kur'an Meali Uygulaması (MAUI)

## 🎯 Projenin Amacı Nedir?

**Nedir?**
Bu proje; C# ve .NET MAUI ile geliştirilmiş, platformlar arası (cross-platform) çalışan, zengin içerikli bir **Kur'an-ı Kerim Okuma ve Karşılaştırma Uygulamasıdır**. 
Uygulamanın temel amacı, 54 farklı çevirmenin meal ve açıklamalarını tek bir ekranda, internet bağlantısına ihtiyaç duymadan, hızlı bir SQLite veritabanı üzerinden sunmaktır.

> **İçerik Hacmi:** Uygulama; 114 sure, ~6.236 ayet ve 54 çevirmen üzerinden toplamda ~336.000 meal ve çevirmene özgü dipnot açıklaması barındırır.

---

## 🛠 Veritabanı ve Teknik Altyapı

Uygulamanın belkemiğini oluşturan `kuran.db` SQLite veritabanı, verimli okuma ve arama yapabilmek için aşağıdaki ilişkisel şema ile tasarlanmıştır:

*   **Sureler:** Sure Numarası (1-114), Türkçe Adı, Ayet Sayısı, İniş Sırası ve Mekkî/Medenî tipi.
*   **Ayetler:** Ayet Numarası, Arapça Metin, Türkçe Transkript.
*   **Mealler:** Çevirmenin site ID'si, Yazar Adı ve temizlenmiş (dipnot referansları kaldırılmış) meal metni.
*   **MealAciklamalari:** Her çevirmenin her ayet için yazdığı tam dipnot açıklamaları.
*   **NuzulSebepleri:** Dipnotlardan ayrıştırılan Esbab-ı Nüzul metinleri.

---
## Çevirmen Listesi (54 Çevirmen)

| ID | Çevirmen |
|---|---|
| golpinarli | Abdulbaki Gölpınarlı Meali |
| aakgul | Abdullah-Ahmet Akgül Meali |
| aparliyan | Abdullah Parlıyan Meali |
| ahmettekin | Ahmet Tekin Meali |
| ahmetvarol | Ahmet Varol Meali |
| abulac | Ali Bulaç Meali |
| alifikriyavuz | Ali Fikri Yavuz Meali |
| bahaeddinsaglam | Bahaeddin Sağlam Meali |
| bayraktar | Bayraktar Bayraklı Meali |
| besimatalay | Besim Atalay Meali (1965) |
| cemalkulunkoglu | Cemal Külünkoğlu Meali |
| cemilsaid | Cemil Said (1924) |
| diyanetislerieski | Diyanet İşleri Meali (Eski) |
| diyanetisleriyeni | Diyanet İşleri Meali (Yeni) |
| kuranyolu | Kur'an Yolu (Diyanet İşleri) |
| diyanetvakfi | Diyanet Vakfı Meali |
| elmalilisade | Elmalılı Hamdi Yazır Meali |
| elmaliliorj | Elmalılı Meali (Orijinal) |
| demiryent | Emrah Demiryent Meali |
| erhanaktas | Erhan Aktaş Meali |
| hbasricantay | Hasan Basri Çantay Meali |
| haydarozturkserkanyilmaz | Haydar Öztürk-Serkan Yılmaz Meali |
| hayrat | Hayrat Neşriyat Meali |
| ihsanaktas | İhsan Aktaş Meali |
| ilyasyorulmaz | İlyas Yorulmaz Meali |
| baltacioglu | İsmayıl Hakkı Baltacıoğlu |
| ismailhakkiizmirli | İsmail Hakkı İzmirli |
| ismailyakit | İsmail Yakıt |
| kadricelik | Kadri Çelik Meali |
| mahmutkisa | Mahmut Kısa Meali |
| mahmutozdemir | Mahmut Özdemir Meali |
| mehmetcakir | Mehmet Çakır Meali |
| mehmetcoban | Mehmet Çoban Meali |
| mehmetokuyan | Mehmet Okuyan Meali |
| mehmetturk | Mehmet Türk Meali |
| muhammedesed | Muhammed Esed Meali |
| mustafacavdar | Mustafa Çavdar Meali |
| islamoglu | Mustafa İslamoğlu Meali |
| orhankuntman | Orhan Kuntman Meali |
| osmanfirat | Osman Fırat Meali |
| omernasuhi | Ömer Nasuhi Bilmen Meali |
| syildirim | Suat Yıldırım Meali |
| sates | Süleyman Ateş Meali |
| suleymantevfik | Süleyman Tevfik (1927) |
| abayindir | Süleymaniye Vakfı Meali |
| spiris | Şaban Piriş Meali |
| simsek | Ümit Şimşek Meali |
| ynuri | Yaşar Nuri Öztürk Meali |
| sardorxonjahongir | Sardorxon Jahongir |
| eskianadoluturkcesi | Eski Anadolu Türkçesi |
| satiralti | Satıraltı Meal (1534) |
| bunyadov | Bunyadov-Memmedeliyev |
| pickthall | M. Pickthall (English) |
| yusufali | Yusuf Ali (English) |
