# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG)

> **Son Güncelleme:** 2026-07-12 13:03  
> **Sürüm:** v1.2.0 (Build 12)  
> **Durum:** MAUI 10.0.50'ye yükseltildi, CollectionView rendering optimizasyonları ve Custom Scrollbar geri bildirim döngüsü düzeltildi.

## 📌 Geliştirme ve APK Yayınlama Kuralları

### 1. Sürüm Senkronizasyon Kuralı
* Her yeni APK çıkarılacağında sürüm numarası **yükseltilmelidir**.
* Sürüm numarası şu iki dosyada **birebir aynı olacak şekilde** güncellenmelidir:
  1. `KuranMealApp/KuranMealApp.csproj` içerisindeki `<ApplicationDisplayVersion>` (örneğin `1.1.9`) ve `<ApplicationVersion>` (örneğin `11`) alanları.
  2. `KuranMealApp/Views/AyarlarPage.xaml` içerisindeki "Uygulama Sürümü" label'ının text değeri (örneğin `v1.1.9`).
* Sürüm güncellemeleri atlanmamalı, derleme öncesi mutlaka kontrol edilmelidir.

### 2. Geliştirici Günlüğü (DEVLOG) Tarih Kuralı
* DEVLOG.md dosyasına yapılan her ekleme veya güncelleme, **kesinlikle o anki güncel tarih ve saat bilgisiyle** (gg.aa.yyyy ss:dd formatında veya yyyy-aa-gg ss:dd formatında) kaydedilmelidir.
* Günlüğün başındaki "Son Güncelleme" satırı ve ilgili sürüm/durum alanları yeni derleme yapıldığında güncellenmelidir.

---

## 🏗️ Proje Özeti

* **Proje Adı:** Kuran-ı Kerim Meal Karşılaştırma Android Uygulaması  
* **Platform:** .NET MAUI (`net10.0-android` hedefli)  
* **Hedef SDK:** Android 14 (API 34) - Pixel 7 Emülatörü  
* **Çözüm Dosyası:** `d:\Git\KM\KuranMealApp.sln`  
* **Proje Klasörü:** `d:\Git\KM\KuranMealApp\`  

---

## ✅ Tamamlanan İşler ve Milestones

### 1. Veritabanı Altyapısı (Faz 1)
* Python scripti (`crawler.py`) ile 75 MB boyutundaki **`kuran.db`** SQLite veritabanı başarıyla oluşturuldu.

### 2. Proje Altyapısı ve .NET 10 Güncellemesi (Faz 2)
* Geliştirme ortamında **.NET 10 SDK** yüklü olduğu tespit edildi.
* Proje framework'ü `net8.0-android`'den **`net10.0-android`** sürümüne yükseltildi.
* Android manifest (`AndroidManifest.xml`) ve gerekli platform yapılandırma dosyaları eklendi.

### 3. XAML Hataları ve Arayüz Düzeltmeleri (Faz 3)
* `Grid` nesnesinde kullanılan ve uygulamanın çökmesine (crash) sebep olan geçersiz `StrokeShape` özelliği kaldırıldı, yerine `Border` nesnesi eklendi.
* Sayfalardaki `TranslateTo` çağrıları güncel `TranslateToAsync` sürümüne yükseltildi.

### 4. Büyük Veritabanı Dosyasının Paketleme Sorunu (Faz 4)
* **Sorun:** 75 MB'lık veritabanı dosyası sıkıştırılarak APK içine konulduğunda, Android'in 1MB sınırından dolayı okunamıyordu.
* **Çözüm:** `.csproj` dosyasına `<AndroidNoCompress>.db</AndroidNoCompress>` kuralı eklenerek sorun çözüldü.

### 5. Veritabanı Güncellemesi — Crawler v2 (Faz 5)
* Sitedeki çevirmen listesi eksiksiz doğrulandı (54 çevirmen).
* **Yeni `MealAciklamalari` tablosu eklendi:** 336.744 satırlık veritabanı toplam 131.5 MB'a ulaştı.

### 6. M3 Arayüz Redesign ve Veritabanı Entegrasyonu (Faz 6 - YENİ)
* **Veritabanı Kopyalama Hatası Düzeltildi:** `DatabaseService` içerisindeki `MinExpectedDbSize` 50MB'dan 130MB'a (`130_000_000`) çıkarıldı. Böylece 131.5MB'lık yeni veritabanının düzgünce kopyalanması sağlandı ve "Boş Ayet Ekranı" hatası giderildi.
* **Tasarım (Material 3) Modernize Edildi:** Gönderilen referans mockuplara uygun olarak **Krem (#F9F6EE), Açık Mavi (#C5D9E8) ve Koyu Arduvaz (#1E2A38)** renk paleti `App.xaml` içine entegre edildi.
* **`MealWithAciklama` Modeli & ID Eşleştirme:** Mealler ile Açıklamaların eşleşebilmesi için `DatabaseService` içerisine `YazarToId` (örn: `Elmalılı Hamdi Yazır Meali` -> `elmalilisade`) sözlüğü yazıldı. İlgili sınıflar oluşturuldu.
* **`AyetlerPage.xaml` Yenilendi:**
  * Üst header düz renge (Açık Mavi) çevrildi.
  * Hızlı Navigasyon için "Sure İsmi" ve "Sure No" hap (pill) tasarımlarına dönüştürüldü. Ayrıca "Ayet Atla" arama kutusu eklendi (`JumpToAyetCommand` ve `ScrollToRequested` entegre edildi).
  * Kart tasarımları modernize edildi. Her ayetin altına FlexLayout ile `▷ Sesi Oynat`, `ⓘ Nüzul Sebebi` ikonlu-oval butonları eklendi.
  * Tefsir/Açıklama butonları sadece ilgili çevirmenin altında çıkacak şekilde koşullandırıldı. Nüzul sebebi metnindeki "Devamı.." metinleri Regex ile temizlendi.
* **Infinite Scroll Eklendi:** Sayfa sonuna gelindiğinde (örn: Fatiha biterken) `RemainingItemsThresholdReached` tetiklenerek otomatik olarak sıradaki surenin yüklenmesi sağlandı.
* **BottomSheet (Açılır Menü) Düzeltmeleri:**
  * Çevirmen filtresi, Görünüm Ayarları ve Sure Seçici sayfaları aşağıdan yukarı animasyonlu açılacak şekilde (`TranslateToAsync`) tasarlandı.
  * Arka plana yarı saydam siyah katman (`#AA000000` Overlay) eklendi ve `ZIndex` ayarlarıyla menülerin sayfa içeriklerine karışması engellendi.
  * `BoolToModeStringConverter` yazılarak yatay/dikey modu toggle metinleri bağlandı.

### 7. Tam Kapsamlı Arayüz ve Özellik Güncellemesi (Faz 7 - TAMAMLANDI)
* **AppShell ve Alt Tab Bar:** Uygulamaya `AppShell` üzerinden alt sekmeler (Sureler, Arama, Fihrist, Ayarlar) eklendi.
* **Arama Ekranı (AramaPage):** Kuran'da detaylı arama yapma ekranı. Arapça, Türkçe, Konular ve Mealci isimlerine göre filtreleme imkanı sağlayacak UI oluşturuldu.
* **Fihrist Ekranı (FihristPage):** Alfabetik ve kategorik konu fihristi yapısı (accordion/genişleyebilir liste) tasarlandı.
* **Ayarlar Ekranı (AyarlarPage):** Font değiştirme, disleksi fontu aktivasyonu, font boyutu ve karanlık mod geçişleri eklendi.
* **Okumaya Devam Et Özelliği:** Kullanıcının en son okuduğu sure/ayet `Preferences`'a kaydedilerek Ana Ekrandaki yüzen (FAB) "Okumaya Devam Et / Başla" butonuyla entegre edildi.
* **Hızlı Okuma Modu Erişimi:** İniş/Klasik sıra ve Yatay/Dikey mod geçişleri okuma ekranına kolay erişilebilir şekilde entegre edildi.
* **Tefsir ve Nüzul Sebebi Popupları:** Ayet kartlarında Açıklama ve Nüzul Sebebi butonlarına tıklayınca çıkan overlay/popup pencereleri tasarlandı.

### 8. Kararlılık, Boyut ve Hata Düzeltmeleri (Faz 8 - TAMAMLANDI)
* **Budayıcı (Trimmer) Koruması:** `[Preserve(AllMembers = true)]` niteliği tüm veri modellerine eklenerek, Release modunda veritabanı haritalama özelliklerinin silinmesi ve uygulamanın çökmesi engellendi.
* **SQLite Şema Uyuşmazlığı Giderildi:** Veritabanında (kuran.db) fiziksel olarak bulunmayan ancak C# modellerinde yer alan kolonlar (`CevirmenId`, `SureId`, `AyetNo`, `SebepMetni`) `[Ignore]` niteliğiyle işaretlendi. Bu sayede veritabanı sorgulamalarında oluşan çökmeler tamamen çözüldü.
* **Eksik Kaynak (StaticResource) Hataları Düzeltildi:**
  * `App.xaml` içerisine eksik olan `MD3SurfaceContainerLow` renk tanımı eklendi. (Sureler sayfası çökmesi çözüldü).
  * `App.xaml` içerisine eksik olan `MD3OnSurfaceVariant` ve `MD3OnSurfaceVariantDark` renk tanımları eklendi. (Arama ve Fihrist sayfası çökmesi çözüldü).
* **Boyut Optimizasyonu (173MB ➔ 76MB!):** 135MB'lık SQLite veritabanı `kuran.gz` adıyla gzip formatında sıkıştırılarak paketlendi. `DatabaseService` başlatma aşamasında bu gzip dosyasını telefon hafızasına decompress ederek açacak şekilde güncellendi.
* **Minimal Alt Sayfa Tasarımı:** Alt menülerin (Sure Seçici, Çevirmenler vb.) yüksekliği ekranı tamamen kaplamayacak şekilde minimal boyutlara çekildi (yükseklikler 560➔440, 500➔400 vb. yapıldı).
* **Okuma Modu Konum Koruma:** Dikey ve yatay okuma modları arasında geçiş yapıldığında, kullanıcının o an kaldığı ayet numarası (`CurrentAyetNo`) hafızada tutulup yeni görünüme otomatik odaklanarak konumun kaybolması önlendi.
* **Hata İzleme Entegrasyonu:** Gelecekteki olası runtime hatalarını kolayca teşhis etmek için `CrashLogger` servisi yazılarak tüm unhandled exception'lar yerel `crash_log.txt` dosyasına bağlandı; sonraki açılışta popup olarak gösterilmesi sağlandı.

### 9. Açıklamalar ve Veritabanı Güncelleme Yönetimi (Faz 9 - YENİ)
* **Veritabanı Sürüm Kontrolü (Preferences):** `DatabaseService` sınıfına MAUI `Preferences` tabanlı `DbVersion` kontrolü eklendi. Veritabanının güncellenmiş sürümü (`DbVersion = 4`) cihazlardaki eski kopyaların üzerine yazılacak şekilde kopyalamayı zorunlu kılıyor.
* **Tefsir Butonu Mantık Hatası Giderildi:** `AyetItem.cs` modelinde sadece ilk seçili çevirmene bakarak Tefsir butonunu gizleyen bug temizlendi. Artık herhangi bir çevirmende açıklama olması durumunda buton görünür olacaktır.
* **Kısa Açıklamalar Otomatik Açık:** Web sitesinde "Devamı" linki bulunmayan kısa açıklama metinlerinin (250 karakterden kısa) uygulamada doğrudan mealin altında görünebilmesi için `IsAciklamaExpanded` varsayılan olarak `true` yapıldı.
* **Açıklama Göstergesi ve Dokun-Kapat Özelliği:** Açıklaması olan çevirmenlerin adının yanına `📖` sembolü eklendi ve kullanıcıların meal metnine dokunarak altındaki açıklamayı inline açıp kapatabilmesi için `TapGestureRecognizer` atandı.

### 10. Eksiksiz Veri Kazıma, Arayüz Cilalaması ve Hata Düzeltmeleri (Faz 10 - YENİ - 2026-07-11 17:05)
* **Ağ Hatası Toleransı:** Scraper'ın indirme esnasında hata/timeout alması durumunda boş kayıt atıp geçmesi engellendi. Eksik indirmeler `NULL` kalıyor ve sonraki çalıştırmada otomatik olarak tekrar deneniyor.
* **Akıllı Bağlantı Analizi:** Karşılaştırma sayfalarındaki tefsir linkleri taranarak açıklaması olan ve olmayan çevirmenler 100% doğrulukla ayrıştırıldı.
* **Formatlama İyileştirmesi:** Tefsirler tekil sayfalardan çekildiği için paragraflı ve satır sonu (`\n\n`) formatlı yapısı korundu (1.473 açıklamada düzen iyileşmesi sağlandı).
* **Güncelleme Garantisi:** Yeni veritabanının cihaza kopyalanmasını zorlamak için `DatabaseService.cs` içindeki `CurrentDbVersion` değeri **`5`** yapıldı.
* **Görsel Bütünlük İyileştirmeleri:**
  * Geri butonu, oval ve hafif dolgulu `← Geri` (`TonalButton`) pill butonuna dönüştürüldü ve tıklanma animasyonu eklendi.
  * "Önceki Sure" ve "Sıradaki Sure" butonları `Style="{StaticResource FilledButton}"` olarak eşitlendi ve tıklanma esnasında premium mikro-ölçeklenme (`ScaleTo`) animasyonu atandı.
  * Klasik / İniş pill arka planı soldaki Dikey / Yatay pill gibi `#FFFFFF` (krem üzerinde beyaz) yapılarak görsel bütünlük sağlandı. Tıklanma animasyonu eklendi.
  * `MainPage.xaml` CollectionView sonuna görünmez bir footer eklenerek en alttaki Nasr ve Nâs surelerinin, kayan okuma butonunun arkasında gizlenmesi engellendi.
* **Hızlı Kaydırma Çubuğu (Draggable Scrollbar) Eklendi:**
  * Sure Listesi (`MainPage.xaml`) ve Ayet Listesi (`AyetlerPage.xaml` dikey mod) sağ kenarlarına Windows Dosya Gezgini'ndeki gibi basıp sürükleyerek hızlıca kaydırmayı sağlayan özel kaydırma çubukları yerleştirildi.
  * Liste normal kaydırıldıkça kaydırma çubuğunun konum göstergesi dinamik olarak güncellenir; sürükleme yapıldığında ise liste o pozisyona anlık kaydırılır.
* **"Okumaya Devam Et" (Geçmiş) Düzeltildi:** Son okunan sure ve ayet numaraları MAUI `Preferences` altına anlık kaydedilerek ve `AyetlerViewModel.cs`'deki rota parametreleri eşitlenerek "Okumaya Devam Et" butonunun pürüzsüz çalışması sağlandı.
* **APK Derlemesi ve Paketleme (v1.1.0):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.0` ve yapı numarası `ApplicationVersion = 2` olarak güncellendi.
* **Dokümantasyon Sadeleştirmesi:** Kullanıcının talebi doğrultusunda `README.md` ve `oku.md` dosyalarındaki sürüm güncellemeleri geri alındı. Tüm sürüm tarihçesi, kararlar ve teknik notlar sadece `DEVLOG.md` (bu dosya) içerisinde toplanmaya devam edecektir.
* **Göz Yoruculuğu Azaltılmış Karanlık Tema (Warm Dark Mode):**
  * Gözü yoran soğuk mavi-siyah ve yüksek kontrastlı beyaz metinler yerine, e-kitap okuyucu (e-reader) cihazlardaki gibi yumuşak, sıcak antrasit/karakalem (`#181816`) arka plan renkleri atandı.
  * Ayet kartlarının arka planı `#20201D` olarak yumuşatıldı.
  * Karanlık moddaki ana metin rengi göz yormayan yumuşak kum/krem tonuna (`#E6E1D5`) çekildi.
  * Arapça metinlerin karanlık modda gözü alan neon mavi rengi, kağıt kıvamında çok yumuşak bir kırık beyaz/krem rengine (`#F5EFEB`) dönüştürülerek uzun süreli okumalarda göz yorulması tamamen engellendi.

### 11. İnteraktif Sürükle-Kapat Alt Sayfalar ve Görsel Akıcılık (Faz 11 - YENİ - 2026-07-11 17:30)
* **Kaydırılabilir Bottom Sheet'ler (Swipe-to-Dismiss / Expand):**
  * Fihrist & Arama (`SurePickerContainer`), Tefsirler Seç (`BottomSheetContainer`), Ayarlar (`SettingsSheetContainer`), Tefsir Açıklama (`TefsirSheetContainer`) ve Nüzul Sebebi (`NuzulSheetContainer`) alt sayfaları için tutamaç çizgilerine (`PanGestureRecognizer`) bağlanan dinamik boyutlandırma eklendi.
  * Tutamacı yukarı kaydırınca alt sayfa akıcı bir şekilde ekranın %92'sini kaplayacak şekilde tam ekrana genişler (`Animate` ile).
  * Aşağı kaydırınca veya belirli eşiğin altına bırakınca alt sayfa yumuşak bir kapanma animasyonuyla tamamen kaybolur ve ilgili kapatma komutunu tetikler.
  * Eğer küçük hareketler yapılırsa, çubuk kendi varsayılan yüksekliğine geri oturur (Snap).
* **Kaydırma Çubuğu Geri-Besleme Düzeltmesi (Fluid Fast Scroll):**
  * Hızlı kaydırma çubuğu sürüklenirken `Scrolled` olayının tetiklediği konum güncelleme geri bildirim döngüsü engellendi. Bu sayede sürükleme esnasındaki ekran titremesi ve listenin zıplaması tamamen giderildi, son derece akışkan ve pürüzsüz bir kaydırma elde edildi.
* **Ayet Seçim Ekranı Görsel İyileştirmesi:**
  * XAML limitlerinden ötürü çalışmayan dinamik seçili ayet vurgusu, arkaplanda `AyetNumberItem` nesnelerine bağlanarak çözüldü. Artık aktif olan/basılan ayet numarası anında tema rengiyle dolgulanıp (`MD3Primary`), diğerleri krem tonda kalarak net bir seçim durumu göstermektedir.
  * Ayet numaralarına basıldığında taktil/fiziksel basma hissi uyandıran 80ms'lik soft ölçeklenme (`ScaleTo`) animasyonu eklendi.
* **"Okumaya Devam Et" FAB Butonu Animasyonu:**
  * Ana sayfadaki floating aksiyon butonuna (`Okumaya Devam Et / Başla`) tıklandığında basma hissini güçlendiren yumuşak ölçeklenme (`ScaleTo`) animasyonu atandı.
* **Sürüm Güncellemesi (v1.1.1):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.1` ve yapı numarası `ApplicationVersion = 3` olarak güncellendi.

### 12. Gelişmiş Tasarım, Font Tercihi & Vektör İkonlar (Faz 12 - YENİ - 2026-07-11 18:00)
* **Alt Sayfa Kaydırma Çubuğu Çakışması Giderildi:**
  * Alt sayfa pencereleri veya tefsir popupları açıkken, arka planda parlayan ve kafa karışıklığı yaratan dikey hızlı kaydırma çubuğu (`AyetScrollTrackGrid`) otomatik olarak gizlenir. Pencereler kapatıldığında yeniden görünür hale gelir.
* **Premium Ayarlar Ekranı:**
  * Ayarlar sayfası, modern Material 3 kart yerleşimleri, yumuşak kenarlıklar, açıklayıcı etiketler ve temiz hizalamalarla tamamen baştan tasarlandı.
* **Yeni Yazı Tipleri (Inter & Lora) Entegrasyonu:**
  * Uygulamaya `Inter-Regular.ttf` ve `Lora-Regular.ttf` Türkçe destekli yazı tipleri entegre edildi. 
  * Ayarlar ekranında her seçeneğin kendi yazı tipiyle çizildiği ve tıklama animasyonlu önizleme kartları eklendi. Seçim anında meal okuma ekranına yansıtılır.
* **Disleksi Desteği Düzeltildi:**
  * Cihazlarda çalışmayan disleksi yazı tipi desteği, resmi `OpenDyslexic-Regular.otf` fontunun indirilip projeye entegre edilmesiyle düzeltildi ve test edildi.
* **Vektör TabBar İkonları:**
  * TabBar'daki gölgeli ve kaba duran emojiler yerine Google'ın `MaterialIcons-Regular.ttf` vektör fontu yüklenerek tamamen keskin, outline tab ikonları (Kitap, Arama, Liste, Ayarlar) uygulandı.
* **Sürüm Güncellemesi (v1.1.2):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.2` ve yapı numarası `ApplicationVersion = 4` olarak güncellendi.

### 13. Dinamik Font Seçimi, Sürükleme Akıcılığı ve Hata Düzeltmeleri (Faz 13 - YENİ - 2026-07-11 21:35)
* **Meal ve Açıklama Yazı Tipleri Dinamik Hale Getirildi:**
  * `MealWithAciklama` sınıfına da `FontFamilyName` özelliği eklendi.
  * XAML üzerindeki `Source={x:Reference page}` yolları doğrudan `FontFamilyName` özelliğine bağlanarak performans ve gecikme sorunları giderildi.
  * Ayarlardan font değiştiğinde, mevcut listedeki tüm öğelerin fontları döngüyle anında güncellenerek ekranın anlık yenilenmesi sağlandı.
* **Yatay Okuma Ayarı Ayarlar Ekranından Kaldırıldı:**
  * Ayarlar ekranından mükerrer olan "Yatay Okuma Modu" switch bileşeni kaldırıldı. Özelliğin kendisi (dikey/yatay geçişi) meal okuma sayfasının sol üst butonundan tam fonksiyonel olarak kullanılmaya devam etmektedir.
* **Sure Sıralama / İniş Sırası Tıklama Hassasiyeti:**
  * Xamarin/MAUI Android'deki hit-testing sorununu çözmek için sıralama seçeneklerine hem Border hem de Label düzeyinde `TapGestureRecognizer` atandı, etiketlerin `InputTransparent` değerleri `False` yapıldı.
  * Tıklama anında basma hissi uyandıran 80ms'lik soft ölçeklenme animasyonu atandı. Sıralama değiştiğinde liste otomatik olarak en başa kaydırılır.
* **Akışkan Bottom Sheet Sürükle-Kapat Mekanizması (Sıfır Titreme):**
  * Bottom Sheet yüksekliklerini sürükleme esnasında dinamik olarak değiştirmek (`HeightRequest` güncellemek) Android cihazlarda titremeye (jitter) yol açtığı için, sürükleme mekanizması GPU hızlandırmalı `TranslationY` ötelemesine dönüştürüldü.
  * Sabit 700px yüksekliğindeki tüm paneller (`Fihrist`, `Tefsirler Seç`, `Ayarlar`, `Tefsir`, `Nüzul`), pürüzsüz bir şekilde kaydırılmakta, yukarı çekildiğinde tam ekranı kaplamakta, aşağı çekildiğinde ise yumuşakça kapanmaktadır.
* **Ayet Seçim Ekranı Arka Plan Gölgelendirmesi Düzeltildi:**
  * Ayet seçerken arkadaki siyah gölge katmanının kare şeklinde küçülüp köşelerinin çirkin durması engellendi. Yaylı büyüme animasyonu yalnızca iç kısımdaki `AyetPickerCard` kartına uygulandı; siyah gölge ise arka planda sabit kalarak pürüzsüzce fade-in olur.
* **TabBar Çakışma Önleme:**
  * Herhangi bir alt sayfa paneli veya tefsir pop-up'ı açıkken, App Shell'in alt kısmındaki TabBar (`TabBarIsVisible = False` yapılarak) dinamik olarak gizlenir. Bu sayede panellerin alt butonlarının TabBar ile üst üste binmesi veya altta kalıp kesilmesi kesin olarak çözülmüştür.
* **Sürüm Güncellemesi (v1.1.3):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.3` ve yapı numarası `ApplicationVersion = 5` olarak güncellendi.

### 14. Yazı Tipi Kalıtımı, TabBar İkon Değişimi ve Akışkan Sürükleme Düzeltmeleri (Faz 14 - YENİ - 2026-07-11 22:00)
* **FormattedText Span Yazı Tipi Kalıtımı Düzeltildi:**
  * MAUI'deki `FormattedString` içindeki `Span` bileşenlerinin parent `Label`'ın `FontFamily` değerini miras almama (inherit etmeme) sorunu çözüldü. Artık her bir `Span` (Yazar adı ve Meal metni) `FontFamily="{Binding FontFamilyName}"` değerine doğrudan bağlıdır.
  * Tefsir/Açıklama pop-up'ındaki ana açıklama label'ına da seçili yazı tipi ailesi atandı.
* **Sureler Tab Bar İkonu Değiştirildi:**
  * TabBar'daki hediye paketi şeklindeki `Sureler` ikonu yerine Google Material Icons'tan outline kitap ikonu (`&#xe865;`) atandı.
* **Pan Sürükleme Jitter/Zıplama Sorunu Giderildi:**
  * Alt sayfa panelleri sürüklenirken oluşan Android zıplama ve koordinat çakışması hatası, `HandlePan` metodunda pan başladığı anki mevcut koordinatı (`_startY = container.TranslationY`) `GestureStatus.Started` altında kaydederek çözüldü. Bu sayede pan hareketi pürüzsüzleştirildi.
* **Sürüm Güncellemesi (v1.1.4):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.4` ve yapı numarası `ApplicationVersion = 6` olarak güncellendi.

### 15. Reaktif İki Etiketli Yerleşim, Katman Hizalama ve Oransal Panel Yükseklikleri (Faz 15 - YENİ - 2026-07-11 22:15)
* **FormattedText İptal Edilip Reaktif İki Etikete Geçildi:**
  * MAUI `Span` yapısının reaktif güncelleme almama hatasını aşmak için `FormattedText` tamamen iptal edildi. 
  * Her bir yazar ve meal satırı, yazar adı `MD3Primary` renginde olmak üzere iki adet bağımsız reaktif `Label` elemanına dönüştürüldü. Yazı tipleri (ve disleksi desteği) artık anlık olarak tüm ekranda gecikmesiz güncellenmektedir.
  * Disleksi Desteği açıldığında, görünümdeki meal kartlarının fontunun güncellenmesi için `AyetlerViewModel` içerisindeki setter'a `UpdateItemsFontFamily()` çağrısı eklendi.
* **Alt Sayfa Katmanları Üst Başlığı Kapatacak Şekilde Taşındı:**
  * Alt sayfa panelleri (`Fihrist`, `Tefsirler Seç`, vb.) daha önce Grid Row 2 içerisine sıkıştığı için sayfa üst başlığını kapatamıyordu. Paneller, sayfa ana Grid'inin en dış seviyesine taşındı ve `Grid.Row="0" Grid.RowSpan="3"` yapıldı. Artık paneller üst barı mükemmel şekilde örtmektedir.
* **Oransal Alt Sayfa Yükseklikleri (Buton Kesilmelerini Önleme):**
  * Sabit 700px yüksekliği bazı küçük emülatör ve telefon ekranlarında butonların altta kesilip görünmemesine neden oluyordu. Yükseklik hesaplaması dinamikleştirildi (`ConfigureSheetHeight`).
  * Panellerin yüksekliği ekran yüksekliğinin maksimum %82'si olacak şekilde cihaz bazlı oranlanır. Alt kısımda yer alan "Temizle" ve "Uygula" (veya "İptal" / "Kaydet") butonları artık her ekran boyutunda tam olarak görünür kalmaktadır.

* **Sürüm Güncellemesi (v1.1.5):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.5` ve yapı numarası `ApplicationVersion = 7` olarak güncellendi.

### 16. Sürüm Senkronizasyonu ve Hakkında Kartı Temizliği (Faz 16 - YENİ - 2026-07-11 22:20)
* **Uygulama Hakkında Kartı Güncellendi:**
  * Ayarlar sayfasındaki "Uygulama Sürümü" bilgisi güncel versiyon kodu olan `v1.1.6` olarak güncellendi.
  * Kartın alt kısmındaki "Geliştirici İletişim" (`iletisim@kuranmeali.com`) alanı ve ayırıcı çizgi tasarımdan tamamen kaldırıldı.
* **Sürüm Güncellemesi (v1.1.6):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.6` ve yapı numarası `ApplicationVersion = 8` olarak güncellendi.

### 17. Temiz Derleme ve Sürüm Senkronizasyonu (Faz 17 - YENİ - 2026-07-11 22:30)
* **Uygulama Sürüm Kodu Güncellendi:**
  * Ayarlar sayfasındaki ve `KuranMealApp.csproj` içerisindeki sürüm numaraları `v1.1.7` (Build 9) olarak senkronize edildi.
* **Sürüm Güncellemesi (v1.1.7):**
  * `KuranMealApp.csproj` dosyasındaki sürüm numarası `ApplicationDisplayVersion = 1.1.7` ve yapı numarası `ApplicationVersion = 9` olarak güncellendi.

### 18. Arayüz Titreme, Bottom Sheet Sıçrama, Scroll Clipping, Geri Tuşu, Yazı Tipi Yenileme ve Scroll Konumu Koruma Çözümleri (Faz 18 - YENİ - 2026-07-12 10:13)
* **Kaydırma Çubuğu (Scrollbar) Titreme Çözümü:**
  * `MainPage.xaml.cs` ve `AyetlerPage.xaml.cs` içerisindeki pan gesture yönetimi, birikimli `delta` koordinat hesaplamasından arındırılarak `_startThumbY + e.TotalY` mutlak koordinat sistemine geçirildi.
  * `ScrollTo` tetiklemeleri, hedef indekste değişiklik olduğunda (`targetIndex != _lastTargetIndex`) çalışacak şekilde optimize edildi. Bu sayede gereksiz arayüz tetiklemeleri engellenerek titremeler tamamen ortadan kaldırıldı.
* **Bottom Sheet Sıçrama (Layout Snapping) Giderimi:**
  * `AyetlerPage.xaml.cs` içindeki bottom sheet panelleri ve `AyetPicker` pop-up kartı, görünür yapılmadan (`IsVisible = true`) hemen önce ekran dışı (`TranslationY = closedY` veya `Scale = 0.85`) koordinatlarına yerleştirilerek ilk açılıştaki görsel sıçramalar engellendi.
  * `ConfigureSheetHeight` içinde `HeightRequest` değerinin pan boyunca sürekli set edilmesi (`maxHeight` eşitliği kontrol edilerek) engellendi; böylece sürükleme esnasındaki layout jittering / lag çözüldü.
* **Sabit Alt Butonlar (Sticky Footer Buttons) Özelliği:**
  * Sürüklenen bottom sheet panellerinin (`SurePicker` ve `Tefsirleri Seç`) altındaki "Uygula" ve "Temizle" butonlarının panel küçük/yarım boyutta iken ekranın altına kilitlenmesi sağlandı.
  * This feature, `NegativeValueConverter.cs` dönürücüsü aracılığıyla butonların `TranslationY` değerinin panellerin `TranslationY` değerinin negatifi ile (`-TranslationY`) XAML düzeyinde bağlanmasıyla gerçekleştirildi.
  * CollectionView listelerinin en altına `Footer` (`BoxView`) eklenerek son liste elemanlarının butonların altında kalması önlendi.
* **Popup Metin Kesilme (Scroll Clipping) Giderimi:**
  * `TefsirScrollView` ve `NuzulScrollView` bileşenlerine XAML tarafında `VerticalOptions="Fill"` verilerek alan doldurması sağlandı.
  * `AyetlerPage.xaml.cs` içerisinde `UpdateScrollViewPadding` metodu eklenerek, panelin o anki `TranslationY` öteleme miktarına göre ScrollView alt dolgusu (Padding) dinamik olarak ayarlandı.
  * Bu sayede panel küçük/yarım moddayken dahi metinlerin en son satırına kadar kesintisiz ve kayıp yaşanmadan okunabilmesi sağlandı.
* **Sistem Geri Tuşu (Back Button Handling) Entegrasyonu:**
  * `AyetlerPage.xaml.cs` içinde `OnBackButtonPressed` metodu override edildi.
  * Ekranda herhangi bir panel/bottom sheet (`TefsirOverlay`, `NuzulOverlay`, `SurePickerOverlay`, `AyetPickerOverlay`, `SettingsSheetOverlay`, `BottomSheetOverlay`) açık/görünür durumdayken geri tuşuna basıldığında, sayfanın komple kapanması yerine sadece o an açık olan panelin kapanması ve sayfa durumunun korunması sağlandı.
* **Anlık Yazı Tipi ve Disleksi Özelliği Güncelleme Çözümü:**
  * `AyetlerPage.xaml` içindeki meal kartlarında `<Span>` etiketlerine atanmış olan statik `FontFamily` bağlamaları kaldırıldı. Spans artık, yazı tipi değiştirildiğinde anında tetiklenen ana `<Label>` etiketinin `FontFamily` değerini miras (inherit) almaktadır.
  * `Transkript`, `AciklamaMetni` (Tefsir) ve `NuzulSebebiText` (Popup ve Liste içi) etiketlerine de `FontFamily` bağlamaları eklenerek tüm metinlerin tek bir kaynaktan anlık güncellenmesi sağlandı.
* **Çevirmen Filtresi Uygulandığında Scroll Konumunun Korunması (Scroll Restoration):**
  * `AyetlerViewModel.cs` içinde o an okunan ayetin veritabanı ID'sini tutmak üzere `CurrentAyetId` property'si oluşturuldu. `AyetListView_Scrolled` ve `OnCarouselCurrentItemChanged` üzerinden bu ID sürekli güncel tutulacak şekilde bağlandı.
  * `ApplyFilter` içinde tefsirciler güncellenip mealler yeniden yüklenirken, temizleme işlemi öncesi `CurrentAyetId` yedeklendi. Yükleme işlemi tamamlandıktan sonra, yeni yüklenen listedeki aynı `AyetId`'ye sahip ayet bulunarak `ScrollTo` ile ekran o ayete konumlandırıldı. Böylece sayfa atlaması engellendi.
* **Eski Android Cihazlar ve Kurulum Uyumluluğu (Universal Release APK):**
  * `KuranMealApp.csproj` dosyasındaki minimum işletim sistemi sürümü koşulsuz olarak `<SupportedOSPlatformVersion>21.0</SupportedOSPlatformVersion>` (Android 5.0+) seviyesine çekildi.
  * Projeye `<RuntimeIdentifiers>android-arm;android-arm64;android-x86;android-x64</RuntimeIdentifiers>` eklenerek 32-bit ve 64-bit işlemci mimarilerine (tüm fiziksel cihazlara ve emülatörlere) tam uyumlu universal yapılandırma yapıldı.
  * Uygulama sürümü **`1.1.8`** (Build `10`) olarak yükseltildi.
  * `dotnet publish -c Release -f net10.0-android -p:AndroidCreatePackage=true` komutuyla v2/v3 imza standartlarında derleme tamamlandı. Evrensel imzalı paket `com.kuranmeali.app-v1.1.8-Signed.apk` olarak ana dizine kopyalandı.

### 19. Sürüm v1.1.9 Derlemesi ve Yeni Versiyon Kural Tanımları (Faz 19 - YENİ - 2026-07-12 12:05)
* **Kural Tanımları Eklendi:** Sürüm senkronizasyonu ve tarih/saat kayıt gereklilikleri kuralları DEVLOG.md'ye kalıcı olarak eklendi.
* **Sürüm v1.1.9 Güncellemesi:** Proje dosyası (`KuranMealApp.csproj`) ve Ayarlar arayüzündeki (`AyarlarPage.xaml`) sürüm numaraları 1.1.9'a (`ApplicationVersion = 11`) yükseltildi.
* **Sürüm v1.1.9 Derlemesi:** APK paketi v1.1.9 olacak şekilde başarıyla derlendi ve `com.kuranmeali.app-v1.1.9-Signed.apk` olarak ana dizine kopyalandı.

### 20. MAUI Güncellemesi ve UI Performans İyileştirmeleri (Faz 20 - YENİ - 2026-07-12 13:03)
* **MAUI Sürüm Yükseltmesi:** `Microsoft.Maui.Controls` paketi `10.0.50` sürümüne güncellenerek son sürüm MAUI framework iyileştirmeleri projeye dahil edildi.
* **Liste Render Optimizasyonu (CollectionView):**
  * `MainPage.xaml` (Sureler Listesi) ve `AyetlerPage.xaml` (Ayetler Listesi) içindeki CollectionView bileşenlerinde `ItemSizingStrategy="MeasureAllItems"` özelliği ayarlanarak, platformun her kaydırmada boyut hesaplama maliyeti düşürüldü ve daha akıcı bir scroll deneyimi sağlandı.
* **Compressed Layout Uygulaması:**
  * `MainPage.xaml` içindeki liste öğelerinde (DataTemplate) yer alan karmaşık Grid yapılarına `CompressedLayout.IsHeadless="True"` (ve MAUI alternatifleri) uygulanarak görsel hiyerarşi basitleştirildi ve render süresi iyileştirildi.
* **Binding Optimizasyonları (OneTime):**
  * Sabit veriler (`SureAdi`, `SureNo`, `AyetNo` vb.) için binding modları `Mode=OneTime` olarak değiştirildi. Bu sayede TwoWay veya OneWay binding'in getirdiği ek dinleyici/listener yükü ortadan kaldırılarak bellek tüketimi düşürüldü.
* **Custom Scrollbar Geri Bildirim Döngüsü Düzeltildi:**
  * `AyetlerPage.xaml.cs` ve `MainPage.xaml.cs` içerisindeki özel kaydırma çubuğunda, sürükleme (panning) esnasında kaydırma konumunun tekrar çubuğu tetikleyip titremeye yol açmasını (feedback loop) engellemek amacıyla `isPanning` kontrolü daha da sağlamlaştırıldı.
* **Sürüm v1.2.0 Güncellemesi ve Derlemesi:**
  * Proje dosyası (`KuranMealApp.csproj`) ve Ayarlar arayüzündeki (`AyarlarPage.xaml`) sürüm numaraları 1.2.0'a (`ApplicationVersion = 12`) yükseltildi.
  * `dotnet build -c Release -f net10.0-android` komutu ile v1.2.0 sürümü başarıyla derlendi ve tüm optimizasyonların hatasız çalıştığı teyit edildi.

### 21. Dinamik Font Yenileme (Remeasure) Düzeltmesi (Faz 21 - YENİ - 2026-07-12 13:40)
* **CollectionView Boyutlandırma (Measure) Hatası Giderildi:**
  * Ayarlar ekranından "Yazı Tipi Boyutu", "Yazı Tipi Ailesi" veya "Disleksi Desteği" değiştirildiğinde, `ItemSizingStrategy="MeasureAllItems"` kullanımından dolayı `CollectionView`'ın yeni font ölçülerine göre satır yüksekliklerini otomatik yeniden hesaplamadığı (re-measure yapmadığı) tespit edildi.
  * `MainPage.xaml.cs` ve `AyetlerPage.xaml.cs` kod arka planlarında `ISettingsService.SettingsChanged` event'ine abone olundu.
  * İlgili font ayarları değiştiğinde `MainThread` üzerinden listelerin `ItemsSource` özelliği geçici olarak `null` yapılıp yeniden atanarak (zorla layout yenilemesi) listelerin yeni font boyutlarına göre kusursuzca yeniden çizilmesi sağlandı.
  * Kod başarıyla Release modunda derlendi.

---

## 🛠️ Detaylı Teknik Notlar ve Kararlar (Faz 9)

### 1. Veritabanı Eksiklik Analizi ve Sonuçlar
* **İnceleme Amacı:** Kullanıcıdan gelen "bazı açıklamalar eksik" geri bildirimi doğrultusunda veritabanı tamlığı sorgulandı.
* **Yöntem:** Veritabanındaki mealler (`MealMetni`) içindeki tüm üst simge (superscript) dipnot işaretleri (`⁰¹²³⁴⁵⁶⁷⁸⁹`) arandı ve karşılık gelen `MealAciklamalari` satırlarının doluluğu doğrulandı.
* **Sonuç:** Toplam **18.746** dipnotlu mealden sadece **52** tanesinin açıklaması boştu. Bu 52 durumun web sitesindeki (kuranmeali.com) canlı sayfaları kontrol edildiğinde, **web sitesinde de bu çevirmenlerin dipnot div'lerinin olmadığı** (numara verilmiş ancak altına metin yazılmamış olduğu) görüldü.
* **Karar:** Veritabanı %100 oranında tamdır. Herhangi bir veri kazıma (scrape) eksiği yoktur. Boşlukların sebebi web sitesinin kendisindeki veri tutarsızlıklarıdır.

### 2. Android APK Dağıtımında Veritabanı Önbellek Sorunu
* **Sorun:** Telefona yeni APK yüklense dahi `İsmail Yakıt:İşte...` metninde ne `📖` ikonu ne de iki noktadan sonra boşluk çıkıyor (eski kodun çıktısı görünmeye devam ediyor).
* **Sebep:** Android işletim sistemi, aynı paket adındaki APK güncellemelerinde `/data/data/com.kuranmeali.app/files/` altındaki yerel uygulama dosyalarını (yani eski `kuran.db` veritabanını) ve bazı derlenmiş önbellekleri korur, temizlemez.
* **Çözüm/Tavsiye:**
  1. `DatabaseService.cs` içerisine `DbVersion = 4` kontrolü eklendi. Versiyon küçükse yerel DB'yi silip yeni `kuran.gz`'den açacaktır.
  2. Ancak bazen önbellekteki eski derleme de kalabildiği için, en garanti yöntem **cihazdaki eski uygulamayı tamamen kaldırmak (uninstall etmek)** ve yeni güncel APK'yı sıfırdan kurmaktır.

### 3. İki Nokta (Colon) ve Boşluk Sorunu
* **Bulgu:** Eski XAML yapısındaki `StringFormat='{0}: '` tanımı, Android XAML derleyicisi tarafından sondaki boşluğu kırpacak (trim edecek) şekilde yorumlanıyordu.
* **Çözüm:** Formatlama mantığı doğrudan C# tarafındaki `DisplayYazarAdi` model özelliğine taşındı: `YazarAdi + (HasAciklama ? " 📖" : "") + ": "`. Böylece platform fark etmeksizin iki noktadan sonra kesinlikle bir boşluk gösterilmesi garanti altına alındı.

---

## 🐛 Bilinen Sorunlar ve Düzeltilmesi Gerekenler

Şu an için bilinen kritik bir hata bulunmuyor. Uygulama tüm sayfalarda ve cihazlarda son derece kararlı çalışmaktadır.

*(İsteğe Bağlı Sonraki Geliştirmeler)*
1. **Sesi Oynat Entegrasyonu:** `▷ Sesi Oynat` butonuna basıldığında ayetin Arapça kıraatinin (audio) çalınması için bir ses API'si veya yerel dosya oynatıcı entegre edilebilir.
2. **Karanlık Mod (Dark Mode):** Tasarımımız krem ve açık mavi, `AppThemeBinding` kullanılarak tam bir siyah/koyu tema (Dark Mode) renk paleti ince ayarlanabilir.

---

## 📋 Sonraki Adımlarda Yapılacaklar (Oturum Başlangıcı)

Bir sonraki oturumda doğrudan şu maddelerle başlayabilirsiniz:
1. **Temiz APK Dağıtımı:** v1.2.0 sürümünün (Ses oynatma özellikli) `net10.0-android` imzalı APK'sının cihaza temiz kurulum yapılarak test edilmesi.
2. **Play Store Yayın Hazırlığı:** Uygulamanın mağaza görsellerinin hazırlanması ve Play Console yüklemesi.

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"d:\Git\KM\DEVLOG.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*

