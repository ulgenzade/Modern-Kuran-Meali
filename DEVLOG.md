# 📖 KuranMealApp — Geliştirici Günlüğü (DEVLOG)

> **Son Güncelleme:** 2026-07-13 15:50  
> **Sürüm:** v1.2.9 (Build 21)  
> **Durum:** Aktif (Faz 21-31).

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
* Devlog dosyası çok uzadığında, her 10 fazda bir yeni bir devlog dosyası açılır ve yeni fazlar oraya kaydedilir. Eski fazlar arşiv dosyalarına (`docs/DEVLOG_1_10.md` vb.) taşınır.

---

## 🏗️ Proje Özeti

* **Proje Adı:** Kuran-ı Kerim Meal Karşılaştırma Android Uygulaması  
* **Platform:** .NET MAUI (`net10.0-android` hedefli)  
* **Hedef SDK:** Android 14 (API 34) - Pixel 7 Emülatörü  
* **Çözüm Dosyası:** `d:\Git\KM\KuranMealApp.sln`  
* **Proje Klasörü:** `d:\Git\KM\KuranMealApp\`  

---

## 📦 Arşivlenmiş Günlükler
* [Faz 1-10](docs/DEVLOG_1_10.md)
* [Faz 11-20](docs/DEVLOG_11_20.md)
* [Faz 21-30](docs/DEVLOG_21_30.md)

---

## 💬 Devam Etmek İçin AI'a Verilecek Komut
> *"docs/DEVLOG.md dosyasını oku, en son nerede kaldığımızı özetle ve sonraki adımlardan ilkiyle başlayalım."*