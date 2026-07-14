# Modern Kuran Meali Veri Kazıma Botu (Crawler) v2

Bu dizin, kuranmeali.com sitesindeki **54 çevirmenin tüm meal ve açıklamalarını** çekip yerel bir SQLite veritabanı (`kuran.db`) oluşturan Python botunu içermektedir.

> **Toplam Beklenen Veri:** ~6.236 ayet × 54 çevirmen = ~336.000 meal + çevirmene özgü dipnot açıklamaları (MealAciklamalari)

## Gereksinimler

Botun çalışabilmesi için sisteminizde **Python 3** yüklü olmalıdır.

Gerekli Python kütüphanelerini yüklemek için:
```bash
pip install -r requirements.txt
```

## Çalıştırma Seçenekleri

### 1. Test Modu (Sadece Fatiha Suresi - Hızlı Test)
Veritabanı şemasının ve veri çekme mantığının doğruluğunu hızlıca kontrol etmek için sadece 1. sureyi (Fatiha) çeker:
```bash
python crawler.py --test
```

### 2. Tüm Veritabanını Çekme (Tam Mod)
Kuran'daki 114 surenin, tüm ayetlerin ve 54 çevirmenin meallerini + açıklamalarını çeker:
```bash
python crawler.py --threads 5
```
*Sunucu yüküne karşı varsayılan 5 thread önerilir. Açıklama çekme thread'i ayrıdır (varsayılan: 3).*

### 3. Sadece Mealleri Çek (Açıklamalar Hariç)
Aciklama.php sayfalarını atlar, yalnızca meal metinlerini kaydeder:
```bash
python crawler.py --threads 5 --skip-aciklama
```

### 4. Sadece Eksik Açıklamaları Çek
Mealler zaten varsa, sadece `MealAciklamalari` tablosundaki eksik kayıtları doldurur:
```bash
python crawler.py --only-aciklama --aciklama-threads 3
```

### 5. Farklı Veritabanı Dosya Yolu Belirtme
Varsayılan olarak `kuran.db` dosyası oluşturulur. Farklı bir dosya adı belirtmek isterseniz:
```bash
python crawler.py --db ozel_kuran.db
```

## Kesinti Durumunda Devam Etme (Resuming)
Eğer veri çekme işlemi internet kesintisi veya başka bir sebepten yarıda kalırsa, betiği **yeniden çalıştırmanız yeterlidir**. Betik, SQLite veritabanındaki kayıtları kontrol ederek otomatik olarak **kaldığı yerden devam edecektir**.

## SQLite Veritabanı Şeması (v2)

Veritabanı (`kuran.db`) aşağıdaki şemaya göre oluşturulur:

```sql
CREATE TABLE Sureler (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SureNo INTEGER UNIQUE,         -- Sure Numarası (1-114)
    SureAdi TEXT,                 -- Türkçe Sure Adı
    AyetSayisi INTEGER,           -- Suredeki Ayet Sayısı
    InisSirasi INTEGER,           -- Nüzul (İniş) Sırası
    Tip TEXT                      -- Mekkî veya Medenî
);

CREATE TABLE Ayetler (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SureId INTEGER,               -- Sureler tablosuna Foreign Key
    AyetNo INTEGER,               -- Ayet Numarası
    InisSirasiNo INTEGER,         -- Ayetin kronolojik önceliği
    ArapcaMetin TEXT,             -- Birleştirilmiş Arapça metin
    Transkript TEXT,              -- Türkçe Latin harfli okunuş
    FOREIGN KEY(SureId) REFERENCES Sureler(Id) ON DELETE CASCADE
);

CREATE TABLE Mealler (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AyetId INTEGER,               -- Ayetler tablosuna Foreign Key
    CevirmenId TEXT,              -- Çevirmenin site ID'si (ör: "diyanetisleriyeni")
    YazarAdi TEXT,                -- Çevirmenin tam adı
    MealMetni TEXT,               -- Temizlenmiş meal metni (dipnot referansları [1][2] kaldırılmış)
    FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
);

-- YENİ: Her çevirmenin her ayet için Aciklama.php'deki tam dipnot açıklaması
CREATE TABLE MealAciklamalari (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AyetId INTEGER,               -- Ayetler tablosuna Foreign Key
    CevirmenId TEXT,              -- Çevirmenin site ID'si
    YazarAdi TEXT,                -- Çevirmenin tam adı
    AciklamaMetni TEXT,           -- Tam açıklama metni (boş string = açıklama yok)
    FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
);

CREATE TABLE NuzulSebepleri (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AyetId INTEGER,               -- Ayetler tablosuna Foreign Key
    Aciklama TEXT,                -- Dipnotlardan çıkarılan Esbab-ı Nüzul metni
    FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
);
```

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
