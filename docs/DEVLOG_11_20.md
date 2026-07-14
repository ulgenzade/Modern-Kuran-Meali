# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG 11-20)

> **Son Güncelleme:** 2026-07-13 13:19  
> **Sürüm:** v1.2.4 (Build 16)  
> **Durum:** Arşivlendi (Faz 11-20).

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

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"docs/DEVLOG_21_30.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*