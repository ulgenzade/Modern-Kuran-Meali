# Proje: Modern Kuran Meali Android Uygulaması (Teknik ve Tasarım Dokümantasyonu)

## 1. Proje Vizyonu ve Stratejik Hedefler
* **Temel Sorun:** Kapsamlı meal, tefsir ve gramer kaynaklarına sahip mevcut web platformlarının (özellikle veri kaynağımız olan kuranmeali.com) arayüzlerinin demode, karmaşık ve mobil uyumluluktan uzak olması.
* **Çözüm:** Veritabanını tamamen yerelde barındıran (offline-first), Material Design 3 standartlarında, yüksek performanslı, akıcı ve kullanıcıyı içeriğe odaklayan modern bir .NET MAUI Android uygulaması geliştirmek.
* **Mühendislik Prensibi:** Yapay bir boyut sınırı gözetilmeksizin, en sorunsuz, takılmayan ve optimize edilmiş çalışma performansı hedeflenecektir. 
* **Veri Kaynağı:** [kuranmeali.com](https://www.kuranmeali.com/)

---

## 2. Teknik Yığın (Tech Stack)
* **Mobil Geliştirme Çatısı:** .NET MAUI (C#, XAML) - Visual Studio (2022 / 2026).
* **Veri Madenciliği ve Bot Altyapısı:** Python (BeautifulSoup, Requests, SQLite3).
* **Yerel Depolama ve Veritabanı:** SQLite.
* **Güvenli Bulut Katmanı (AI Proxy):** Supabase Edge Functions veya hafif bir PHP/Python backend mimarisi.

---

## 3. Veri Mimarisi ve Veritabanı Tasarımı

### A. Python Veri Kazıma Botunun Görevleri
* Orijinal Arapça metin ve Türkçe Transkript.
* İsmail Yakıt, Diyanet İşleri vb. seçilen hocaların mealleri.
* Ayetlere ait Esbab-ı Nüzul (İniş Sebebi) ve surelerin kronolojik İniş (Nüzul) Sırası numaraları.
* Kelime kelime irab ve Türkçe anlam analizleri (opsiyonel kelime meali modülü için).

### B. SQLite Veritabanı Şeması (Optimize Edilmiş)
* `Sureler` (Id, SureNo, SureAdi, AyetSayisi, InisSirasi)
* `Ayetler` (Id, SureId, AyetNo, InisSirasiNo, ArapcaMetin, Transkript)
* `Mealler` (Id, AyetId, YazarAdi, MealMetni)
* `NuzulSebepleri` (Id, AyetId, Aciklama)

---

## 4. UI/UX Tasarım Dili ve Modüler Ekran Mimarisi

### A. Gelişmiş Okuma Modları ve Evrensel Filtreleme
Uygulamada iki temel sıralama düzeni bulunacaktır:
1. **Standart Mushaf Sırası Modu:** Geleneksel dizilime göre (Fatiha, Bakara...) okuma.
2. **Kronolojik İniş (Nüzul) Sırası Modu:** Tarihsel sürece göre (Alak, Kalem...) okuma.

* **Evrensel Çeviri Filtresi (Bottom Sheet):** Kullanıcı "Filtrele" butonuna bastığında açılan modern menüden hocaları seçer. Kapatıldığı anda, bulunulan mod hangisi olursa olsun (Mushaf veya Nüzul), ayet listesi anında sadece seçilen meallerle güncellenir. 

### B. Akış, Yönelim ve Erişilebilirlik
* **Yatay Okuma Modu (Carousel/Swipe):** Ayetler tam ekran kartlar halindedir. Sağa/sola kaydırarak sayfa çevrilir.
* **Dikey Okuma Modu (Webtoon/Akış):** Ayet kartları dikey düzlemde alt alta listelenir. Sadece aşağı kaydırarak kesintisiz okuma yapılır.
* **Erişilebilirlik (A11y):** Kullanıcılar font ailelerini değiştirebilir. Okuma güçlüğü çekenler için "OpenDyslexic" font seçeneği sunulacaktır.
* **Karanlık Tema (Dark Mode):** Gece mavisi ve koyu antrasit tonlarıyla, parlamayı önleyen mat kırık beyaz metin renkleri tam entegre çalışacaktır.

---

## 5. İleri Düzey Fonksiyonel Modüller

### A. Sesli Okuma (Tilavet)
* Uygulama yükünü hafif tutmak için sesler internet üzerinden (stream) oynatılır.
* **Çevrimdışı Dinleme:** Ayarlar menüsünden tüm ses paketleri yerel depolamaya indirilebilir.

### B. Güvenli AI Destekli Tefsir ve Anlam Asistanı
* Sadece aktif internet bağlantısıyla çalışır. Cihaz donanımını yormamak adına işlemler bulutta çözülür.
* **Güvenlik Kapısı:** API anahtarları Android uygulamasına gömülmez. İstekler güvenli bir bulut fonksiyonu (Supabase/PHP) üzerinden yapay zekaya yönlendirilir.
* **Bağlam Beslemeli Soru-Cevap (RAG):** Kullanıcının sorusu, yerel SQLite veritabanındaki Arapça metin, meal ve tefsir verileriyle birleştirilerek yapay zekaya gönderilir. Nokta atışı ve veritabanına sadık cevaplar alınır.

---

## 6. Aşama Aşama Geliştirme Planı
* **Faz 1: Veri Madenciliği (Scraping):** Python botunun yazılması ve `kuran.db` SQLite dosyasının üretilmesi.
* **Faz 2: Çekirdek Mobil Altyapı:** .NET MAUI projesinin ayağa kaldırılması ve SQLite entegrasyonu.
* **Faz 3: UI/UX İnşası:** Material Design 3 ekranları, Evrensel Filtreleme, Yatay/Dikey okuma modları, Disleksi fontu ve Karanlık Tema optimizasyonları.
* **Faz 4: Medya ve AI Entegrasyonu:** Ses oynatıcı (MediaElement) yöneticisi ve güvenli aracı sunucu bağlantısıyla AI asistanının devreye alınması.


gerekli istedikelerini de söyle bana