# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG 1-10)

> **Son Güncelleme:** 2026-07-13 13:19  
> **Sürüm:** v1.2.4 (Build 16)  
> **Durum:** Arşivlendi (Faz 1-10).

## 📌 Geliştirme ve APK Yayınlama Kuralları

### 1. Sürüm Senkronizasyon Kuralı
* Her yeni APK çıkarılacağında sürüm numarası **yükseltilmelidir**.
* Sürüm numarası şu iki dosyada **birebir aynı olacak şekilde** güncellenmelidir:
  1. `KuranMealApp/KuranMealApp.csproj` içerisindeki `<ApplicationDisplayVersion>` (örneğin `1.1.9`) ve `<ApplicationVersion>` (örneğin `11`) alanları.
  2. `KuranMealApp/Views/AyarlarPage.xaml` içerisindeki "Uygulama Sürümü" label'ının text değeri (örneğin `v1.1.9`).
* Sürüm güncellemeleri atlanmamalı, derleme öncesi mutlaka kontrol edilmelidir.
* **APK Dağıtım Kuralı:** Derlenen APK sadece iki yere gider: `KuranMealApp/bin/Release/net10.0-android/` (sabit isimle) ve `D:\Git\KM\apks\` (sürüm numaralı isimle, örn. `com.kuranmeali.app-v1.2.4-Signed.apk`). Başka hiçbir yere kopyalanmaz, yeni klasör oluşturulmaz.

### 2. Geliştirici Günlüğü (DEVLOG) Tarih Kuralı
* DEVLOG.md dosyasına yapılan her ekleme veya güncelleme, **kesinlikle o anki güncel tarih ve saat bilgisiyle** (gg.aa.yyyy ss:dd formatında veya yyyy-aa-gg ss:dd formatında) kaydedilmelidir.
* Günlüğün başındaki "Son Güncelleme" satırı ve ilgili sürüm/durum alanları yeni derleme yapıldığında güncellenmelidir.

### 3. Devlog Bölme Kuralı
* Devlog dosyası çok uzadığında, her 10 fazda bir yeni bir devlog dosyası açılır ve yeni fazlar oraya kaydedilir.

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

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"docs/DEVLOG.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*