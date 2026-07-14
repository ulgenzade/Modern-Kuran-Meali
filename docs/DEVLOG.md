# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG)

> **Son Güncelleme:** 2026-07-14 01:08  
> **Sürüm:** v1.3.4 (Build 24)  
> **Durum:** Aktif (Faz 21-33).

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
* Devlog dosyası çok uzadığında, her 10 fazda bir yeni bir devlog dosyası açılır ve yeni fazlar oraya kaydedilir. Eski fazlar arşiv dosyalarına (`DEVLOG_1_10.md` vb.) taşınır.

---

## 🏗️ Proje Özeti

* **Proje Adı:** Kuran-ı Kerim Meal Karşılaştırma Android Uygulaması  
* **Platform:** .NET MAUI (`net10.0-android` hedefli)  
* **Hedef SDK:** Android 14 (API 34) - Pixel 7 Emülatörü  
* **Çözüm Dosyası:** `d:\Git\KM\KuranMealApp.sln`  
* **Proje Klasörü:** `d:\Git\KM\KuranMealApp\`  

---

## 📦 Arşivlenmiş Günlükler
* [Faz 1-10](DEVLOG_1_10.md)
* [Faz 11-20](DEVLOG_11_20.md)

---

## ✅ Tamamlanan İşler ve Milestones (Aktif)

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

### 27. Ayarlar Anlık Uygulama ve Tasarım İyileştirmeleri (Faz 27 - YENİ - 2026-07-13 13:55)
* **Anlık Ayar Uygulama:** `AyarlarViewModel`'den Shell reset işlemi kaldırıldı. `DynamicResource` ve `SettingsService.ApplySettingsToResources()` ile font ve boyut değişiklikleri anlık hale getirildi.
* **Karanlık Mod Fix:** `SettingsService.IsDarkMode` setter'ına `Application.Current.UserAppTheme` güncellemesi eklenerek temanın anında değişmesi sağlandı.
* **Font Kalıtım Çözümü:** `AyetlerPage.xaml` içerisindeki meal metinleri için `{DynamicResource AppFontFamily}` yerine doğrudan modelden gelen `FontFamilyName` binding'i kullanılarak font değişimlerinin tüm satırlarda kesin çalışması sağlandı.
* **M3 Tasarım Güncellemesi:** `App.xaml` içerisinde `M3PillSecondary` stili, Material 3 standartlarına uygun olarak outline (kenarlık) ve `MD3SecondaryContainer` rengiyle güncellendi.

### 28. Font Binding Düzeltmeleri ve v1.2.5 Sürümü (Faz 28 - YENİ - 2026-07-13 15:02)
* (Önceki notlar...)

### 29. Ayarların "Uygula" Butonu ile Senkronizasyonu (Faz 29 - YENİ - 2026-07-13 15:15)
* **Geçici Ayar Mekanizması:** `AyarlarViewModel` içerisindeki `IsDarkMode`, `UseDyslexicFont`, `FontSizeScale` ve `SelectedFontFamily` özellikleri, `SettingsService`'e anlık yazmak yerine "Uygula" butonuna basılana kadar geçici (temporary) tutulacak şekilde refactor edildi.
* **Uygula Butonu:** `AyarlarPage` üzerindeki "Uygula" butonu, `ApplyCommand` ile tüm ayarları `SettingsService`'e tek seferde yazar. `SettingsService`'in `ApplySettingsToResources()` metodu tetiklenerek `DynamicResource` değerleri (font ailesi ve boyutları) global olarak güncellenir.
* **Binding Düzeltmeleri:** `AyarlarViewModel`'de eksik olan `FontSizeDisplay` property'si eklendi. `AyarlarPage` üzerindeki buton binding'leri düzeltildi.

### 30. Sürüm v1.2.7 ve APK Dağıtımı (Faz 30 - YENİ - 2026-07-13 15:18)
* **Sürüm Güncellemesi:** `KuranMealApp.csproj` ve `AyarlarPage.xaml` dosyalarında sürüm `v1.2.7` (Build 18) olarak güncellendi.
* **APK Derlemesi:** Proje Release modunda derlendi, `com.kuranmeali.app-v1.2.7-Signed.apk` oluşturuldu ve `apks/` klasörüne taşındı.

### 31. Ayarlar Sayfası Border Düzeltmesi, Sürüm v1.2.8 ve APK Dağıtımı (Faz 31 - YENİ - 2026-07-13 15:39)
* **XAML Border Hatası Giderildi:** `AyarlarPage.xaml` içindeki Grid bileşenlerinde `Grid.StrokeShape` kullanımı MAUI tarafından desteklenmediği için derleme hatasına yol açıyordu. Grid'ler Border bileşenleri içerisine alınarak `Border.StrokeShape` kullanılacak şekilde güncellendi ve XAML derleme hatası çözüldü.
* **Sürüm Güncellemesi:** `KuranMealApp.csproj` ve `AyarlarPage.xaml` dosyalarında sürüm `v1.2.8` (Build 20) olarak güncellendi.
* **APK Derlemesi:** Proje Release modunda derlendi, `com.kuranmeali.app-v1.2.8-Signed.apk` oluşturuldu ve `apks/` klasörüne taşındı.

### 32. Font Ayarlarının Temizlenmesi (Faz 32 - YENİ - 2026-07-13 17:30)
* **Font Ayarları Kaldırıldı:** `AyarlarViewModel`, `AyarlarPage.xaml`, `ISettingsService` ve `SettingsService` üzerinden font boyutu ve font ailesi değiştirme özellikleri temizlendi.
* **Kod Temizliği:** İlgili UI bileşenleri ve servis metodları projeden arındırıldı.

### 33. Arama Ekranı Tefsir Seçim Paneli ve v1.3.4 (Faz 33 - YENİ - 2026-07-14 01:09)
* **Arama Bottom-Sheet Yenileme:** `AramaPage.xaml` üzerindeki "Tefsirleri Seç" alt paneli, `AyetlerPage` referansı ile birebir aynı genişletilebilir yapıya kavuşturuldu. `TranslatorPickerContainer` yüksekliği `700` yapıldı; `ConfigureSheetHeight` ve `HandlePan` metotları entegre edildi.
* **Buton Konumlandırması:** Alt butonların yer aldığı `Border` `padding="16,12,16,24"` olarak ayarlandı ve `TranslationY` binding'i eklenerek butonlar sheet ile birlikte hareket eder hale getirildi.
* **Sürüm Güncellemesi:** `KuranMealApp.csproj` ve `AyarlarPage.xaml` dosyalarında sürüm `v1.3.4` (Build 24) olarak güncellendi.
* **APK Derlemesi:** Proje Release modunda derlendi, `com.kuranmeali.app-v1.3.4-Signed.apk` oluşturuldu ve `apks/` klasörüne taşındı.

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"docs/DEVLOG.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*
