"""
KuranMeali.com Kapsamli Veri Kazima Botu v2
============================================
Ceker:
  - 114 Sure bilgisi (Siralama.php)
  - 6236 Ayet (Arapca metin + Transkript)
  - 54 cevirmene ait tum mealler (AyetKarsilastirma.php)
  - Her cevirmen+ayet icin aciklamalar (Aciklama.php) -> MealAciklamalari tablosu
  - Nuzul sebepleri (dipnotlardan) -> NuzulSebepleri tablosu

Kullanim:
  python crawler.py                     # Tum veriyi ceker (5 thread)
  python crawler.py --threads 3         # 3 thread ile
  python crawler.py --test              # Sadece Fatiha (1. sure)
  python crawler.py --db ozel.db        # Farkli dosya adi
  python crawler.py --skip-aciklama     # Aciklamalari atla, sadece mealler
  python crawler.py --only-aciklama     # Sadece eksik aciklamalari cek
  python crawler.py --aciklama-threads 2
"""

import os
import re
import sys
import time
import argparse
import sqlite3
import random
import threading
from urllib.parse import urljoin
from concurrent.futures import ThreadPoolExecutor, as_completed
import requests
from bs4 import BeautifulSoup

BASE_URL = "https://www.kuranmeali.com/"

# Sitedeki tum cevirmen ID'leri (AraForm.php select option value'lari)
# abayindir = Suleymaniye Vakfi Meali
TRANSLATOR_IDS = [
    "golpinarli",            # Abdulbaki Golpinarli Meali
    "aakgul",                # Abdullah-Ahmet Akgul Meali
    "aparliyan",             # Abdullah Parliyan Meali
    "ahmettekin",            # Ahmet Tekin Meali
    "ahmetvarol",            # Ahmet Varol Meali
    "abulac",                # Ali Bulac Meali
    "alifikriyavuz",         # Ali Fikri Yavuz Meali
    "bahaeddinsaglam",       # Bahaeddin Saglam Meali
    "bayraktar",             # Bayraktar Bayrakh Meali
    "besimatalay",           # Besim Atalay Meali (1965)
    "cemalkulunkoglu",       # Cemal Kulunkoglu Meali
    "cemilsaid",             # Cemil Said (1924)
    "diyanetislerieski",     # Diyanet Isleri Meali (Eski)
    "diyanetisleriyeni",     # Diyanet Isleri Meali (Yeni)
    "kuranyolu",             # Kur'an Yolu (Diyanet Isleri)
    "diyanetvakfi",          # Diyanet Vakfi Meali
    "elmalilisade",          # Elmahh Hamdi Yazir Meali
    "elmaliliorj",           # Elmahh Meali (Orijinal)
    "demiryent",             # Emrah Demiryent Meali
    "erhanaktas",            # Erhan Aktas Meali
    "hbasricantay",          # Hasan Basri Cantay Meali
    "haydarozturkserkanyilmaz",  # Haydar Ozturk-Serkan Yilmaz Meali
    "hayrat",                # Hayrat Nesriyat Meali
    "ihsanaktas",            # Ihsan Aktas Meali
    "ilyasyorulmaz",         # Ilyas Yorulmaz Meali
    "baltacioglu",           # Ismayil Hakki Baltacioglu
    "ismailhakkiizmirli",    # Ismail Hakki Izmirli
    "ismailyakit",           # Ismail Yakit
    "kadricelik",            # Kadri Celik Meali
    "mahmutkisa",            # Mahmut Kisa Meali
    "mahmutozdemir",         # Mahmut Ozdemir Meali
    "mehmetcakir",           # Mehmet Cakir Meali
    "mehmetcoban",           # Mehmet Coban Meali
    "mehmetokuyan",          # Mehmet Okuyan Meali
    "mehmetturk",            # Mehmet Turk Meali
    "muhammedesed",          # Muhammed Esed Meali
    "mustafacavdar",         # Mustafa Cavdar Meali
    "islamoglu",             # Mustafa Islamoglu Meali
    "orhankuntman",          # Orhan Kuntman Meali
    "osmanfirat",            # Osman Firat Meali
    "omernasuhi",            # Omer Nasuhi Bilmen Meali
    "syildirim",             # Suat Yildirim Meali
    "sates",                 # Suleyman Ates Meali
    "suleymantevfik",        # Suleyman Tevfik (1927)
    "abayindir",             # Suleymaniye Vakfi Meali
    "spiris",                # Saban Piris Meali
    "simsek",                # Umit Simsek Meali
    "ynuri",                 # Yasar Nuri Ozturk Meali
    "sardorxonjahongir",     # Sardorxon Jahongir
    "eskianadoluturkcesi",   # Eski Anadolu Turkcesi
    "satiralti",             # Satiralti Meal (1534)
    "bunyadov",              # Bunyadov-Memmedeliyev
    "pickthall",             # M. Pickthall (English)
    "yusufali",              # Yusuf Ali (English)
]

USER_AGENTS = [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0"
]

# Thread-local session for connection reuse per thread
_thread_local = threading.local()


def get_session():
    if not hasattr(_thread_local, "session"):
        s = requests.Session()
        s.headers.update({
            "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            "Accept-Language": "tr,en-US;q=0.7,en;q=0.3",
            "Connection": "keep-alive",
        })
        _thread_local.session = s
    return _thread_local.session


def get_headers():
    return {"User-Agent": random.choice(USER_AGENTS)}


# ---------------------------------------------------------------------------
# Database Setup
# ---------------------------------------------------------------------------

def init_db(db_path):
    """Tum tablolari olusturur ve baglanti doner."""
    conn = sqlite3.connect(db_path, check_same_thread=False)
    conn.execute("PRAGMA foreign_keys = ON;")
    conn.execute("PRAGMA journal_mode = WAL;")
    conn.execute("PRAGMA synchronous = NORMAL;")
    conn.execute("PRAGMA cache_size = -65536;")  # 64 MB
    cursor = conn.cursor()

    cursor.execute("""
    CREATE TABLE IF NOT EXISTS Sureler (
        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
        SureNo      INTEGER UNIQUE,
        SureAdi     TEXT,
        AyetSayisi  INTEGER,
        InisSirasi  INTEGER,
        Tip         TEXT
    );
    """)

    cursor.execute("""
    CREATE TABLE IF NOT EXISTS Ayetler (
        Id           INTEGER PRIMARY KEY AUTOINCREMENT,
        SureId       INTEGER,
        AyetNo       INTEGER,
        InisSirasiNo INTEGER,
        ArapcaMetin  TEXT,
        Transkript   TEXT,
        FOREIGN KEY(SureId) REFERENCES Sureler(Id) ON DELETE CASCADE
    );
    """)

    cursor.execute("""
    CREATE TABLE IF NOT EXISTS Mealler (
        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
        AyetId      INTEGER,
        CevirmenId  TEXT,
        YazarAdi    TEXT,
        MealMetni   TEXT,
        FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
    );
    """)

    # Tam aciklama metni (Aciklama.php'den)
    cursor.execute("""
    CREATE TABLE IF NOT EXISTS MealAciklamalari (
        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
        AyetId         INTEGER,
        CevirmenId     TEXT,
        YazarAdi       TEXT,
        AciklamaMetni  TEXT,
        FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
    );
    """)

    cursor.execute("""
    CREATE TABLE IF NOT EXISTS NuzulSebepleri (
        Id        INTEGER PRIMARY KEY AUTOINCREMENT,
        AyetId    INTEGER,
        Aciklama  TEXT,
        FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
    );
    """)

    # Performans icin indeksler
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_ayetler_sureid         ON Ayetler(SureId);")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_mealler_ayetid         ON Mealler(AyetId);")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_mealler_cevirmenid     ON Mealler(CevirmenId);")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_aciklama_ayetid        ON MealAciklamalari(AyetId);")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_aciklama_cevirmen      ON MealAciklamalari(CevirmenId);")
    cursor.execute("CREATE UNIQUE INDEX IF NOT EXISTS idx_aciklama_unique ON MealAciklamalari(AyetId, CevirmenId);")

    conn.commit()
    print(f"[DB] Veritabani hazir: {db_path}")
    return conn


# ---------------------------------------------------------------------------
# HTTP Helpers
# ---------------------------------------------------------------------------

def fetch_url(url, retries=6, backoff_factor=2):
    """URL'yi ceker; basarisizlikta exponential backoff ile yeniden dener."""
    session = get_session()
    for attempt in range(retries):
        try:
            time.sleep(random.uniform(0.15, 0.55))
            r = session.get(url, headers=get_headers(), timeout=20)
            if r.status_code == 200:
                r.encoding = "utf-8"
                return r.text
            elif r.status_code == 429 or r.status_code >= 500:
                wait = backoff_factor ** attempt + random.uniform(1, 3)
                print(f"  [HTTP {r.status_code}] {url} — {wait:.1f}s bekleniyor...")
                time.sleep(wait)
            else:
                print(f"  [HTTP {r.status_code}] {url}")
                return None
        except requests.exceptions.Timeout:
            wait = backoff_factor ** attempt + random.uniform(1, 2)
            print(f"  [Timeout] Deneme {attempt+1}/{retries}, {wait:.1f}s bekleniyor: {url}")
            time.sleep(wait)
        except Exception as exc:
            wait = backoff_factor ** attempt + random.uniform(0.5, 1.5)
            print(f"  [Hata] {exc} ({attempt+1}/{retries}), {wait:.1f}s: {url}")
            time.sleep(wait)
    print(f"  [BASARISIZ] {retries} denemeden sonra vazgecildi: {url}")
    return None


# ---------------------------------------------------------------------------
# Parsing Helpers
# ---------------------------------------------------------------------------

def clean_html(text):
    if not text:
        return ""
    cleaned = re.sub(r"<[^>]+>", "", text)
    return re.sub(r"\s+", " ", cleaned).strip()


def clean_meal_text(text):
    """Dipnot referanslarini ([1], [2], ...) kaldirir."""
    if not text:
        return ""
    text = re.sub(r"\[\d+\]", "", text)
    return re.sub(r"\s+", " ", text).strip()


# ---------------------------------------------------------------------------
# Surah Scraper
# ---------------------------------------------------------------------------

def scrape_surahs(db_conn):
    cursor = db_conn.cursor()
    cursor.execute("SELECT COUNT(*) FROM Sureler")
    if cursor.fetchone()[0] == 114:
        print("[Sure] 114 sure zaten mevcut, atlaniyor.")
        return

    print("[Sure] Siralama.php'den sure listesi cekiliyor...")
    html = fetch_url(urljoin(BASE_URL, "Siralama.php"))
    if not html:
        print("[HATA] Siralama.php alinamadi!")
        sys.exit(1)

    soup = BeautifulSoup(html, "html.parser")
    rows = soup.find_all("tr", style=lambda s: s and "border-bottom" in s and "dashed" in s)

    surahs = []
    for row in rows:
        cells = row.find_all("td")
        if len(cells) < 4:
            continue

        a_tag = cells[0].find("a", href=lambda h: h and "Sayfalar.php" in h)
        if not a_tag:
            continue

        sure_adi = a_tag.text.strip().split("/")[0].strip()
        sure_no = int(cells[1].text.strip())

        nuzul_text = cells[2].text.strip()
        m = re.match(r"(\d+)-(.*)", nuzul_text)
        if m:
            inis_sirasi, tip = int(m.group(1)), m.group(2).strip()
        else:
            inis_sirasi = int(nuzul_text) if nuzul_text.isdigit() else 0
            tip = "Mekki"

        ayet_sayisi = int(cells[3].text.strip())
        surahs.append((sure_no, sure_adi, ayet_sayisi, inis_sirasi, tip))

    if len(surahs) != 114:
        print(f"[Uyari] {len(surahs)} sure cekilebildi (114 bekleniyor).")

    cursor.executemany("""
    INSERT OR REPLACE INTO Sureler (SureNo, SureAdi, AyetSayisi, InisSirasi, Tip)
    VALUES (?, ?, ?, ?, ?)
    """, surahs)
    db_conn.commit()
    print(f"[Sure] {len(surahs)} sure kaydedildi.")


# ---------------------------------------------------------------------------
# Ayet Page Parser
# ---------------------------------------------------------------------------

def parse_ayet_page(html):
    """
    AyetKarsilastirma.php sayfasini parse eder.
    Doner: dict(arabic_text, transcript_text, meals)
    meals: [(cevirmen_id, yazar_adi, meal_metni, footnote_data)]
    """
    if not html:
        return None

    soup = BeautifulSoup(html, "html.parser")

    # --- Arapca metin ---
    arabic_text = ""
    div = soup.find(id="arapca")
    if div:
        cdivs = div.find_all("div", recursive=False)
        if len(cdivs) >= 2:
            font = cdivs[1].find("font", dir="rtl")
            if font:
                words = [a.text.strip() for a in font.find_all("a") if a.text.strip()]
                arabic_text = " ".join(words) if words else font.text.strip()
            else:
                arabic_text = cdivs[1].text.strip()

    # --- Transkript ---
    transcript_text = ""
    div = soup.find(id="transliteration")
    if div:
        cdivs = div.find_all("div", recursive=False)
        if len(cdivs) >= 2:
            span = cdivs[1].find("span")
            transcript_text = clean_html(str(span)) if span else clean_html(cdivs[1].text)

    # --- Mealler ---
    meals = []

    for cid in TRANSLATOR_IDS:
        div = soup.find(id=cid)
        if not div:
            continue

        cdivs = div.find_all("div", recursive=False)
        if len(cdivs) < 2:
            continue

        yazar_adi = clean_html(cdivs[0].text)
        
        # Check if there is an explanation link
        has_aciklama = False
        a_tags = cdivs[1].find_all("a", href=lambda h: h and "Aciklama.php" in h and "meal=" in h)
        if a_tags:
            has_aciklama = True

        # BS4 uzerinde kopya olustur
        text_soup = BeautifulSoup(str(cdivs[1]), "html.parser")

        # Dipnot div'i (font-size:12px) bulalım
        fn_div = text_soup.find("div", style=lambda s: s and "font-size:12px" in s)
        if fn_div:
            fn_div.decompose()

        # Aciklama linki ve kirmizi span'lari temizle
        for tag in text_soup.find_all("a", href=lambda h: h and "Aciklama.php" in h):
            tag.decompose()
        for tag in text_soup.find_all("span", style=lambda s: s and "color:red" in s):
            tag.decompose()

        meal_text = clean_meal_text(clean_html(text_soup.get_text()))
        if meal_text:
            meals.append((cid, yazar_adi, meal_text, has_aciklama))

    return {
        "arabic_text": arabic_text,
        "transcript_text": transcript_text,
        "meals": meals,
    }


def scrape_single_ayet(sure_no, ayet_no):
    url = urljoin(BASE_URL, f"AyetKarsilastirma.php?sure={sure_no}&ayet={ayet_no}")
    html = fetch_url(url)
    if not html:
        return None
    try:
        return parse_ayet_page(html)
    except Exception as exc:
        print(f"  [Parse Hatasi] Sure {sure_no} Ayet {ayet_no}: {exc}")
        return None


# ---------------------------------------------------------------------------
# Aciklama (Footnote) Page Fetcher
# ---------------------------------------------------------------------------

def fetch_aciklama(cevirmen_id, sure_no, ayet_no):
    """
    Aciklama.php'den tam aciklama metnini ceker.
    Doner: (success, text)
    """
    url = urljoin(BASE_URL, f"Aciklama.php?meal={cevirmen_id}&sureno={sure_no}&ayet={ayet_no}")
    html = fetch_url(url)
    if html is None:
        return False, None

    soup = BeautifulSoup(html, "html.parser")

    # Tablonun icindeki td'leri tara
    paragraphs = []
    for td in soup.find_all("td"):
        # Baslik td'sini atla (bold font iceriyor)
        if td.find("b") and td.find("font"):
            continue
        # "Kapat" butonunu atla
        if td.find("input", {"value": "Kapat"}):
            continue
        text = td.get_text(separator="\n").strip()
        if text:
            paragraphs.append(text)

    full = "\n\n".join(paragraphs)
    full = re.sub(r"\n{3,}", "\n\n", full).strip()
    return True, full


# ---------------------------------------------------------------------------
# DB Write Helpers (thread-safe)
# ---------------------------------------------------------------------------

_db_lock = threading.Lock()


def save_ayet_to_db(db_conn, sure_no, ayet_no, data):
    with _db_lock:
        cursor = db_conn.cursor()
        cursor.execute("SELECT Id, InisSirasi FROM Sureler WHERE SureNo = ?", (sure_no,))
        row = cursor.fetchone()
        if not row:
            print(f"  [Hata] Sure {sure_no} veritabaninda bulunamadi!")
            return False, None
        sure_id, inis_sirasi = row

        try:
            cursor.execute("BEGIN TRANSACTION;")
            cursor.execute("""
            INSERT INTO Ayetler (SureId, AyetNo, InisSirasiNo, ArapcaMetin, Transkript)
            VALUES (?, ?, ?, ?, ?)
            """, (sure_id, ayet_no, inis_sirasi, data["arabic_text"], data["transcript_text"]))
            ayet_id = cursor.lastrowid

            cursor.executemany("""
            INSERT INTO Mealler (AyetId, CevirmenId, YazarAdi, MealMetni)
            VALUES (?, ?, ?, ?)
            """, [(ayet_id, cid, yazar, metin) for cid, yazar, metin, has_aciklama in data["meals"]])

            for cid, yazar, metin, has_aciklama in data["meals"]:
                if not has_aciklama:
                    # Hic aciklama yok, bos kayit ekle
                    cursor.execute("""
                    INSERT INTO MealAciklamalari (AyetId, CevirmenId, YazarAdi, AciklamaMetni)
                    VALUES (?, ?, ?, ?)
                    """, (ayet_id, cid, yazar, ""))
                else:
                    # Aciklama var, NULL olarak isaretle (Adim 3'te çekilecek)
                    cursor.execute("""
                    INSERT INTO MealAciklamalari (AyetId, CevirmenId, YazarAdi, AciklamaMetni)
                    VALUES (?, ?, ?, NULL)
                    """, (ayet_id, cid, yazar))

            cursor.execute("COMMIT;")
            return True, ayet_id
        except Exception as exc:
            cursor.execute("ROLLBACK;")
            print(f"  [DB Hatasi] Sure {sure_no} Ayet {ayet_no}: {exc}")
            return False, None


def save_aciklama_to_db(db_conn, ayet_id, cevirmen_id, yazar_adi, aciklama_metni):
    with _db_lock:
        cursor = db_conn.cursor()
        try:
            cursor.execute("""
            INSERT OR REPLACE INTO MealAciklamalari (AyetId, CevirmenId, YazarAdi, AciklamaMetni)
            VALUES (?, ?, ?, ?)
            """, (ayet_id, cevirmen_id, yazar_adi, aciklama_metni))
            db_conn.commit()
            return True
        except Exception as exc:
            print(f"  [Aciklama DB Hatasi] AyetId={ayet_id} [{cevirmen_id}]: {exc}")
            return False


def fetch_and_save_aciklama(db_conn, ayet_id, cevirmen_id, yazar_adi, sure_no, ayet_no):
    success, aciklama = fetch_aciklama(cevirmen_id, sure_no, ayet_no)
    if success:
        save_aciklama_to_db(db_conn, ayet_id, cevirmen_id, yazar_adi, aciklama or "")
        return True
    else:
        return False


# ---------------------------------------------------------------------------
# Queue Helpers
# ---------------------------------------------------------------------------

def get_pending_ayets(db_conn, limit_to_fatiha=False):
    cursor = db_conn.cursor()
    if limit_to_fatiha:
        cursor.execute("SELECT SureNo, AyetSayisi FROM Sureler WHERE SureNo = 1")
    else:
        cursor.execute("SELECT SureNo, AyetSayisi FROM Sureler ORDER BY SureNo")
    surah_counts = cursor.fetchall()

    all_expected = [
        (s, a)
        for s, cnt in surah_counts
        for a in range(1, cnt + 1)
    ]

    cursor.execute("""
    SELECT Sureler.SureNo, Ayetler.AyetNo
    FROM Ayetler JOIN Sureler ON Ayetler.SureId = Sureler.Id
    """)
    completed = set(cursor.fetchall())

    pending = [x for x in all_expected if x not in completed]
    return pending, len(all_expected)


def get_pending_aciklamalar(db_conn, limit_to_fatiha=False):
    """MealAciklamalari'nda AciklamaMetni NULL olan (ayet, cevirmen) ciftlerini doner."""
    cursor = db_conn.cursor()
    sure_filter = "AND S.SureNo = 1" if limit_to_fatiha else ""
    cursor.execute(f"""
    SELECT MA.AyetId, MA.CevirmenId, MA.YazarAdi, S.SureNo, A.AyetNo
    FROM   MealAciklamalari MA
    JOIN   Ayetler A ON MA.AyetId = A.Id
    JOIN   Sureler S ON A.SureId = S.Id
    WHERE  MA.AciklamaMetni IS NULL
    {sure_filter}
    ORDER BY S.SureNo, A.AyetNo, MA.CevirmenId
    """)
    return cursor.fetchall()


# ---------------------------------------------------------------------------
# Stats
# ---------------------------------------------------------------------------

def print_stats(db_conn):
    cursor = db_conn.cursor()
    print("\n=== Veritabani Istatistikleri ===")
    for table in ["Sureler", "Ayetler", "Mealler", "MealAciklamalari", "NuzulSebepleri"]:
        cursor.execute(f"SELECT COUNT(*) FROM {table}")
        print(f"  {table:28s}: {cursor.fetchone()[0]:>12,} satir")
    print("=" * 46)


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="KuranMeali.com Kapsamli Veri Kazima Botu v2",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("--db", default="kuran.db", help="SQLite dosya yolu (varsayilan: kuran.db)")
    parser.add_argument("--threads", type=int, default=5, help="Ayet cekme thread sayisi (varsayilan: 5)")
    parser.add_argument("--test", action="store_true", help="Test modu: sadece Fatiha (1. sure)")
    parser.add_argument("--skip-aciklama", action="store_true", help="Aciklamalari atla")
    parser.add_argument("--only-aciklama", action="store_true", help="Sadece eksik aciklamalari cek")
    parser.add_argument("--aciklama-threads", type=int, default=3,
                        help="Aciklama cekme thread sayisi (varsayilan: 3)")
    args = parser.parse_args()

    print("=" * 60)
    print("  KuranMeali.com Kapsamli Veri Kazima Botu v2")
    print("=" * 60)
    print(f"  Veritabani       : {args.db}")
    print(f"  Cevirmen Sayisi  : {len(TRANSLATOR_IDS)}")
    print(f"  Ayet Thread      : {args.threads}")
    print(f"  Aciklama Thread  : {args.aciklama_threads}")
    print(f"  Test Modu        : {'Evet (Sadece Fatiha)' if args.test else 'Hayir (Tum Kuran)'}")
    print(f"  Aciklamalar      : {'Atlanacak' if args.skip_aciklama else 'Cekilecek'}")
    print("=" * 60)

    conn = init_db(args.db)

    # Adim 1: Sure listesi
    scrape_surahs(conn)

    # Adim 2: Ayet mealleri
    if not args.only_aciklama:
        pending, total = get_pending_ayets(conn, limit_to_fatiha=args.test)
        done_count = total - len(pending)

        print(f"\n[Ayet] Toplam hedef : {total:,}")
        print(f"[Ayet] Tamamlanan   : {done_count:,}")
        print(f"[Ayet] Bekleyen     : {len(pending):,}")

        if pending:
            print(f"\n[Ayet] {args.threads} thread ile cekme basliyor...\n")
            try:
                with ThreadPoolExecutor(max_workers=args.threads) as executor:
                    future_map = {
                        executor.submit(scrape_single_ayet, s, a): (s, a)
                        for s, a in pending
                    }
                    counter = done_count
                    for future in as_completed(future_map):
                        sure, ayet = future_map[future]
                        try:
                            data = future.result()
                            if data:
                                ok, ayet_id = save_ayet_to_db(conn, sure, ayet, data)
                                if ok:
                                    counter += 1
                                    pct = counter / total * 100
                                    print(
                                        f"  [{pct:6.2f}%] Sure {sure:3d} Ayet {ayet:3d}"
                                        f" | {len(data['meals']):2d} meal"
                                        f" ({counter:,}/{total:,})"
                                    )
                                else:
                                    print(f"  [HATA] Sure {sure} Ayet {ayet} kaydedilemedi.")
                            else:
                                print(f"  [HATA] Sure {sure} Ayet {ayet} verisi alinamadi.")
                        except Exception as exc:
                            print(f"  [ISTISNA] Sure {sure} Ayet {ayet}: {exc}")
            except KeyboardInterrupt:
                print("\n[Uyari] Kullanici tarafindan durduruldu.")
        else:
            print("[Ayet] Tum ayetler zaten mevcut.")

    # Adim 3: Aciklamalar (Aciklama.php)
    if not args.skip_aciklama:
        print(f"\n[Aciklama] Eksik kayitlar sorgulanıyor...")
        pending_ac = get_pending_aciklamalar(conn, limit_to_fatiha=args.test)
        total_ac = len(pending_ac)

        print(f"[Aciklama] Kontrol edilecek kayit : {total_ac:,}")
        print(f"[Aciklama] (Aciklamasi olmayan cevirmenler icin bos kayit eklenir,")
        print(f"            bu sayede bir sonraki calistirmada tekrar denenmez.)")
        print(f"[Aciklama] {args.aciklama_threads} thread ile basliyor...\n")

        if pending_ac:
            found_count = 0
            done_ac = 0
            try:
                with ThreadPoolExecutor(max_workers=args.aciklama_threads) as executor:
                    future_map = {
                        executor.submit(
                            fetch_and_save_aciklama,
                            conn, ayet_id, cid, yazar, sure_no, ayet_no
                        ): (ayet_id, cid, sure_no, ayet_no)
                        for ayet_id, cid, yazar, sure_no, ayet_no in pending_ac
                    }
                    for future in as_completed(future_map):
                        ayet_id, cid, sure_no, ayet_no = future_map[future]
                        done_ac += 1
                        try:
                            found = future.result()
                            if found:
                                found_count += 1
                                pct = done_ac / total_ac * 100
                                print(
                                    f"  [{pct:6.2f}%] ACIKLAMA Sure {sure_no} Ayet {ayet_no}"
                                    f" [{cid}] ({done_ac:,}/{total_ac:,})"
                                )
                            else:
                                # Hiz amaciyla sadece her 200'de bir yazdir
                                if done_ac % 200 == 0:
                                    pct = done_ac / total_ac * 100
                                    print(
                                        f"  [{pct:6.2f}%] Isleniyor... ({done_ac:,}/{total_ac:,},"
                                        f" {found_count} aciklama bulundu)"
                                    )
                        except Exception as exc:
                            print(f"  [ISTISNA] Sure {sure_no} Ayet {ayet_no} [{cid}]: {exc}")
            except KeyboardInterrupt:
                print("\n[Uyari] Kullanici tarafindan durduruldu.")

            print(f"\n[Aciklama] Tamamlandi: {found_count:,} aciklama bulunup kaydedildi.")
        else:
            print("[Aciklama] Tum aciklamalar zaten mevcut.")

    rebuild_nuzul_table(conn)
    print_stats(conn)
    conn.close()
    print("\n[Tamamlandi] Veritabani baglantisi kapatildi.")
    print("=" * 60)


def rebuild_nuzul_table(db_conn):
    print("\n[Nuzul] NuzulSebepleri tablosu yeniden olusturuluyor...")
    cursor = db_conn.cursor()
    cursor.execute("DELETE FROM NuzulSebepleri;")
    
    cursor.execute("SELECT AyetId, AciklamaMetni FROM MealAciklamalari WHERE AciklamaMetni != ''")
    rows = cursor.fetchall()
    
    keywords = ["nuzul", "nazil", "inis sebebi", "esbab", "sebebiyle in", "munasebeti", "nüzul", "nâzil", "iniş sebebi", "münasebeti"]
    inserted_pairs = set()
    nuzul_rows = []
    for ayet_id, text in rows:
        text_lower = text.lower()
        if any(kw in text_lower for kw in keywords):
            clean_text = text.strip()
            if (ayet_id, clean_text) not in inserted_pairs:
                nuzul_rows.append((ayet_id, clean_text))
                inserted_pairs.add((ayet_id, clean_text))
                
    cursor.executemany("""
    INSERT INTO NuzulSebepleri (AyetId, Aciklama)
    VALUES (?, ?)
    """, nuzul_rows)
    db_conn.commit()
    print(f"[Nuzul] {len(nuzul_rows)} adet temiz nuzul sebebi kaydedildi.")


if __name__ == "__main__":
    main()
