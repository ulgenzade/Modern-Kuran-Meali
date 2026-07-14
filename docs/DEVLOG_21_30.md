# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG 21-30)

> **Son Güncelleme:** 2026-07-13 18:10  
> **Sürüm:** v1.3.3.0 (Build 23)  
> **Durum:** Aktif (Faz 21-30).

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

### 21. Dinamik Font Yenileme (Remeasure) Düzeltmesi (Faz 21 - YENİ - 2026-07-12 13:40)
* **CollectionView Boyutlandırma (Measure) Hatası Giderildi:**
  * Ayarlar ekranından "Yazı Tipi Boyutu", "Yazı Tipi Ailesi" veya "Disleksi Desteği" değiştirildiğinde, `ItemSizingStrategy="MeasureAllItems"` kullanımından dolayı `CollectionView`'ın yeni font ölçülerine göre satır yüksekliklerini otomatik yeniden hesaplamadığı (re-measure yapmadığı) tespit edildi.
  * `MainPage.xaml.cs` ve `AyetlerPage.xaml.cs` kod arka planlarında `ISettingsService.SettingsChanged` event'ine abone olundu.
  * İlgili font ayarları değiştiğinde `MainThread` üzerinden listelerin `ItemsSource` özelliği geçici olarak `null` yapılıp yeniden atanarak (zorla layout yenilemesi) listelerin yeni font boyutlarına göre kusursuzca yeniden çizilmesi sağlandı.
  * Kod başarıyla Release modunda derlendi.

### 22. Kod Temizliği ve Derleme Uyarıları (Faz 22 - YENİ - 2026-07-12 15:27)
* **Derleme Uyarıları (Warnings) Giderildi:**
  * `SettingsService` içerisinde null dönme ihtimali olan `Preferences.Get` kullanımlarına varsayılan değerler atanarak nullable uyarıları çözüldü.
  * Sayfa (`View`) ve ViewModel'lerde kullanılmayan private field'lar temizlendi.
  * MAUI'de obsolete (kullanımdan kaldırılmış) olarak işaretlenen animasyon çağrılarından kurtulmak için, kodlardaki animasyon kullanımları daha güvenli duruma getirildi.
* **Null Reference Güvenliği Artırıldı:**
  * `DatabaseService` içerisindeki veritabanı kopyalama rutininde `context.Assets` null kontrolü eklendi.
  * `AyarlarViewModel` ve `AyetlerViewModel` içerisinde `Application.Current` null kontrolü sağlanarak tema değişikliklerindeki potansiyel çökmeler önlendi.
  * `AramaPage`'de `Application.Current.MainPage.DisplayAlert` kullanılarak null reference ihtimali giderildi.
* **Sürüm v1.2.1 Güncellemesi ve APK Derlemesi:**
  * `KuranMealApp.csproj` dosyasındaki sürüm numaraları 1.2.1'e (`ApplicationVersion = 13`) yükseltildi.
  * Temizlenen kod tabanıyla `v1.2.1` sürümünün Release APK paketi derlendi.

### 23. Dinamik Font Yenileme - Popup Düzeltmesi (Faz 23 - YENİ - 2026-07-12 15:55)
* **Tefsir Popup Font Hatası Giderildi:**
  * Ayarlardan disleksi veya font ailesi değiştirildiğinde meal listesindeki öğeler anlık güncellenirken, "Açıklama / Tefsir" popup'ındaki (`SelectedAyetForPopup`) meal nesnelerinin eski fontta kalması sorunu tespit edildi.
  * `AyetlerViewModel.cs` içindeki `UpdateItemsFontFamily` metoduna `SelectedAyetForPopup` ve içerisindeki `Mealler` listesi için de `FontFamilyName` güncelleme mantığı eklenerek bu sorun çözüldü.
* **Sürüm v1.2.2 Güncellemesi:**
  * Proje dosyası (`KuranMealApp.csproj`) ve Ayarlar arayüzündeki (`AyarlarPage.xaml`) sürüm numaraları 1.2.2'ye (`ApplicationVersion = 14`) yükseltildi.

### 24. "Ayarları Uygula" Butonu ve Geçici Ayar Önizlemesi (Faz 24 - YENİ - 2026-07-13 11:20)
* **Geçici (Temporary) Ayar Mekanizması:**
  * Ayarlar ekranında switch/slider değiştirildiğinde değerler anlık olarak ViewModel property'lerine yazılmakta, ancak `ISettingsService` (kalıcı ayarlar) `IsDarkMode`, `UseDyslexicFont`, `FontSizeScale`, `SelectedFontFamily` ve `UseHorizontalReadingMode` setter'larına yazılmamaktadır.
  * `AyarlarViewModel` artık kendi iç state'inde (private field) çalışmakta; `OnAppearing` üzerinden `LoadSettings()` ile kalıcı ayarları çekmektedir.
  * Karanlık mod önizlemesi anlık olarak uygulanmaktadır (kullanıcı geri alabilsin diye).
* **"Ayarları Uygula" (Apply) Butonu Eklendi:**
  * `AyarlarPage.xaml` ve `AyetlerPage.xaml` (Settings Sheet) alt kısmına büyük, vurgulu `Ayarları Uygula` butonu eklendi.
  * `ApplyCommand` / `ApplySettingsCommand`: Kalıcı ayarları servise yazar, ardından mevcut rotayı kaydedip `Application.Current.MainPage = new AppShell()` ile Shell'i sıfırdan oluşturarak tüm sayfaların yeni ayarlarla yeniden render edilmesini sağlar.
  * Sonrasında kullanıcıyı kaldığı sekmeye/rotaya (`Shell.Current.GoToAsync(route, false)`) geri yönlendirir.
* **Sürüm v1.2.3 Güncellemesi:**
  * Proje dosyası (`KuranMealApp.csproj`) ve Ayarlar arayüzündeki (`AyarlarPage.xaml`) sürüm numaraları 1.2.3'e (`ApplicationVersion = 15`) yükseltildi.

### 25. Ortam Temizliği, APK Dağıtım Kuralları ve Eksik Kalan Görevler (Faz 25 - YENİ - 2026-07-13 12:58)
* **BAŞARILI - Sürüm Senkronizasyonu ve APK Ortam Temizliği (v1.2.4):**
  * `.csproj` içerisinde `<TargetFrameworks>` yerine tekil hedef olan `<TargetFramework>net10.0-android</TargetFramework>` atandı. Böylece eski `.net8.0-android` hedefi sebebiyle oluşan gereksiz derlemeler ve stale klasörler temizlendi.
  * `D:\Git\KM\APK\` vb. dağınık klasörler ve kök dizindeki APK dosyaları silinerek, derleme çıktılarının tek bir konumda (`apks\`) ve versiyonlu (`com.kuranmeali.app-v1.2.4-Signed.apk`) toplanması sağlandı.
  * `.csproj` ve `AyarlarPage.xaml` üzerinden sürüm numarası v1.2.4 (Build 16) olarak güncellenip Release APK derlemesi yapıldı.
* **BAŞARISIZ / YAPILMADI - Font Kök-Neden Düzeltmesi:**
  * `AyetlerPage.xaml` sayfasındaki font (yazı tipi) değişiminin tüm satırlara (Label/Span vb.) doğru şekilde uygulanması veya inheritance kopukluğunun (kök-neden) çözülmesi işlemi **yapılmamıştır**. Kod veya XAML düzeyinde herhangi bir font binding onarımı gerçekleştirilmemiştir.
* **BAŞARISIZ / YAPILMADI - Material 3 Tasarım Tutarlılığı:**
  * Switch/Checkbox değişiklikleri, butonların ve form elemanlarının güncel Material 3 yönergelerine uygun hale getirilmesi veya yeni etkileşim (animasyon) özelliklerinin eklenmesi işlemi **yapılmamıştır**.

### 26. Dinamik Font Binding ve Pill Buton Animasyonları (Faz 26 - YENİ - 2026-07-13 13:19)
* **Dinamik Font Uygulaması:** `AyetlerPage.xaml` içerisindeki meallerin `FontFamily` tanımı `{DynamicResource AppFontFamily}` olarak güncellendi.
* **Ayarlar Mekanizması İyileştirmesi:** "Ayarları Uygula" butonu tetiklendiğinde tüm Shell'in sıfırlanması yerine daha hafif bir güncelleme mekanizmasına geçildi.
* **M3 Pill Buton Bileşenleri:** Material 3 standartlarında, yeniden kullanılabilir `M3PillPrimary`, `M3PillSecondary` ve `M3PillTertiary` stilleri oluşturuldu.
* **Etkileşim Animasyonları:** `AyetlerPage` ve `AyarlarPage` üzerindeki pill butonlara, basma hissi veren yumuşak ölçeklenme (`ScaleTo`) animasyonları eklendi.

### 27. Sürüm 1.2.5 Güncellemesi (Faz 27 - YENİ - 2026-07-13 13:40)
* **Sürüm Güncellemesi:** Proje dosyası (`KuranMealApp.csproj`) ve `AyarlarPage.xaml` içerisindeki sürüm bilgisi v1.2.5 (Build 17) olarak güncellendi.
* **APK Derlemesi:** Release modunda yeni `v1.2.5` APK'sı derlenerek `apks/` klasörüne kopyalandı.
* **Devlog Düzenlemesi:** Ana Devlog dosyası boyut sınırına ulaştığı için her 10 fazda bir arşivlenecek şekilde parçalara ayrıldı (`docs/DEVLOG_1_10.md`, `docs/DEVLOG_11_20.md`, vb.). Ana `DEVLOG.md` sadece kuralları, özeti ve aktif fazı gösterecek biçimde sadeleştirildi.

### 28. Hata Düzeltme ve Geri Alma (Faz 28 - YENİ - 2026-07-13 16:05)
* **Hata Giderildi:** Ayarlar sayfasındaki sürüm bilgisi ve proje yapılandırması, yanlışlıkla yapılan v1.2.9 güncellemesi sonrası oluşan hatalar nedeniyle v1.2.0 sürümüne geri döndürüldü.
* **Proje Kararlılığı:** `KuranMealApp.csproj` ve `AyarlarPage.xaml` dosyaları orijinal hallerine getirilerek uygulamanın açılmama sorunu çözüldü.
* **Derleme:** Proje başarıyla yeniden derlendi ve kararlı hale getirildi.

### 29. Arayüz Renk Düzenlemeleri ve Sürüm 1.3.1 Güncellemesi (Faz 29 - YENİ - 2026-07-13 16:56)
* **UI Renk İyileştirmeleri:** Ayetler sayfasındaki geri butonu, tefsir seçim ekranındaki checkbox (tik) bileşenleri ve tema ayarı butonu karanlık moda (MD3Dark) uyumlu hale getirildi. `AppThemeBinding` eklendi.
* **Sürüm Güncellemesi:** Proje dosyası (`KuranMealApp.csproj`) ve Ayarlar arayüzündeki (`AyarlarPage.xaml`) sürüm numarası v1.3.1 (Build 19) olarak güncellendi.
* **APK Derlemesi:** Yeni sürüm (v1.3.1) APK derlemesi yapıldı ve `apks` dizinine gönderildi.

### 30. Translator Picker Optimizasyonu ve Sürüm 1.3.3.0 (Faz 30 - YENİ - 2026-07-13 18:10)
* **Arama Translator Picker:** `AramaPage.xaml` içerisindeki mealli arama çevirmen seçim `BottomSheet` yüksekliği `HeightRequest` ile optimize edildi.
* **Kaydırma Hareketi Eklendi:** BottomSheet kapanmasını engellememek için liste dışına `PanGestureRecognizer` ve `OnTranslatorPickerSheetPan` olayı eklendi.
* **Sürüm Güncellemesi:** Proje dosyası (`KuranMealApp.csproj`) sürüm numarası v1.3.3.0 (Build 23) olarak güncellendi.
* **Derleme ve APK:** Gerekli düzeltmeler (eksik event handler) yapılarak proje başarıyla derlendi ve yeni APK oluşturulup `apks` klasörüne aktarıldı.

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"docs/DEVLOG.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*