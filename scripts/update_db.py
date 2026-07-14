import os
import re
import sys
import time
import sqlite3
import random
import threading
from urllib.parse import urljoin
from concurrent.futures import ThreadPoolExecutor, as_completed

import requests
from bs4 import BeautifulSoup

sys.stdout.reconfigure(encoding='utf-8')

BASE_URL = "https://www.kuranmeali.com/"
DB_PATH = "kuran.db"
RESUME_FILE = "processed_ayets.txt"

# 54 translators mapping
ID_TO_YAZAR = {
    "golpinarli": "Abdulbaki Gölpınarlı Meali",
    "aparliyan": "Abdullah Parlıyan Meali",
    "aakgul": "Abdullah-Ahmet Akgül Meali",
    "ahmettekin": "Ahmet Tekin Meali",
    "ahmetvarol": "Ahmet Varol Meali",
    "abulac": "Ali Bulaç Meali",
    "alifikriyavuz": "Ali Fikri Yavuz Meali",
    "bahaeddinsaglam": "Bahaeddin Sağlam Meali",
    "bayraktar": "Bayraktar Bayraklı Meali",
    "besimatalay": "Besim Atalay Meali (1965)",
    "bunyadov": "Bunyadov-Memmedeliyev",
    "cemalkulunkoglu": "Cemal Külünkoğlu Meali",
    "cemilsaid": "Cemil Said (1924)",
    "diyanetvakfi": "Diyanet Vakfı Meali",
    "diyanetislerieski": "Diyanet İşleri Meali (Eski)",
    "diyanetisleriyeni": "Diyanet İşleri Meali (Yeni)",
    "elmalilisade": "Elmalılı Hamdi Yazır Meali",
    "elmaliliorj": "Elmalılı Meali (Orijinal)",
    "demiryent": "Emrah Demiryent Meali",
    "erhanaktas": "Erhan Aktaş Meali",
    "eskianadoluturkcesi": "Eski Anadolu Türkçesi",
    "hbasricantay": "Hasan Basri Çantay Meali",
    "haydarozturkserkanyilmaz": "Haydar Öztürk-Serkan Yılmaz Meali",
    "hayrat": "Hayrat Neşriyat Meali",
    "ihsanaktas": "İhsan Aktaş Meali",
    "ilyasyorulmaz": "İlyas Yorulmaz Meali",
    "baltacioglu": "İsmayıl Hakkı Baltacıoğlu",
    "ismailhakkiizmirli": "İsmail Hakkı İzmirli",
    "ismailyakit": "İsmail Yakıt",
    "kadricelik": "Kadri Çelik Meali",
    "kuranyolu": "Kur'an Yolu (Diyanet İşleri)",
    "pickthall": "M. Pickthall (English)",
    "mahmutkisa": "Mahmut Kısa Meali",
    "mahmutozdemir": "Mahmut Özdemir Meali",
    "mehmetcakir": "Mehmet Çakır Meali",
    "mehmetcoban": "Mehmet Çoban Meali",
    "mehmetokuyan": "Mehmet Okuyan Meali",
    "mehmetturk": "Mehmet Türk Meali",
    "muhammedesed": "Muhammed Esed Meali",
    "mustafacavdar": "Mustafa Çavdar Meali",
    "islamoglu": "Mustafa İslamoğlu Meali",
    "orhankuntman": "Orhan Kuntman Meali",
    "osmanfirat": "Osman Fırat Meali",
    "sardorxonjahongir": "Sardorxon Jahongir",
    "satiralti": "Satıraltı Meal (1534)",
    "syildirim": "Suat Yıldırım Meali",
    "sates": "Süleyman Ateş Meali",
    "suleymantevfik": "Süleyman Tevfik (1927)",
    "abayindir": "Süleymaniye Vakfı Meali",
    "spiris": "Şaban Piriş Meali",
    "simsek": "Ümit Şimşek Meali",
    "ynuri": "Yaşar Nuri Öztürk Meali",
    "yusufali": "Yusuf Ali (English)",
    "omernasuhi": "Ömer Nasuhi Bilmen Meali",
}

USER_AGENTS = [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 Chrome/124 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36 Edg/124",
]

_thread_local = threading.local()

def get_session():
    if not hasattr(_thread_local, "sess"):
        s = requests.Session()
        s.headers.update({
            "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            "Accept-Language": "tr,en;q=0.5",
        })
        _thread_local.sess = s
    return _thread_local.sess

def fetch_url(url, retries=5, backoff=2):
    session = get_session()
    for attempt in range(retries):
        try:
            time.sleep(random.uniform(0.1, 0.3))
            r = session.get(url, headers={"User-Agent": random.choice(USER_AGENTS)}, timeout=15)
            if r.status_code == 200:
                r.encoding = "utf-8"
                return r.text
            elif r.status_code in (429, 500, 502, 503, 504):
                wait = backoff ** attempt + random.uniform(1, 3)
                print(f"  [HTTP {r.status_code}] {wait:.1f}s bekleniyor: {url}")
                time.sleep(wait)
            else:
                return None
        except Exception as exc:
            wait = backoff ** attempt + random.uniform(1, 2)
            print(f"  [Hata] {exc} ({attempt+1}/{retries}): {url}")
            time.sleep(wait)
    raise Exception(f"URL yüklenemedi: {url}")

def is_truncated(text):
    if not text:
        return False
    t = text.strip()
    # Check if ends with truncation markers
    if t.endswith('Devamı') or t.endswith('Devamı..') or t.endswith('Devamı...') or t.endswith('...'):
        return True
    last_part = t[-15:]
    if 'Devamı' in last_part or '...' in last_part:
        return True
    return False

def parse_aciklama_page(html):
    if not html:
        return ""
    soup = BeautifulSoup(html, "html.parser")
    paragraphs = []
    for td in soup.find_all("td"):
        if td.find("b") and td.find("font"):
            continue
        if td.find("input", {"value": "Kapat"}):
            continue
        text = td.get_text(separator="\n").strip()
        if text:
            paragraphs.append(text)
    if not paragraphs:
        return ""
    full = "\n\n".join(paragraphs)
    full = re.sub(r"\n{3,}", "\n\n", full).strip()
    return full

def get_existing_explanation(db_path, ayet_id, cevirmen_id):
    """Veritabanında halihazırda tam bir açıklama olup olmadığını döner."""
    conn = sqlite3.connect(db_path)
    c = conn.cursor()
    c.execute("""
        SELECT AciklamaMetni FROM MealAciklamalari 
        WHERE AyetId = ? AND CevirmenId = ?
    """, (ayet_id, cevirmen_id))
    row = c.fetchone()
    conn.close()
    if row and row[0]:
        text = row[0].strip()
        if not is_truncated(text):
            return text
    return None

def process_verse(db_path, ayet_id, sure_no, ayet_no):
    url = f"{BASE_URL}AyetKarsilastirma.php?sure={sure_no}&ayet={ayet_no}"
    html = fetch_url(url)
    if not html:
        return ayet_id, sure_no, ayet_no, []

    soup = BeautifulSoup(html, "html.parser")
    results = []

    for cid in ID_TO_YAZAR.keys():
        div = soup.find(id=cid)
        if not div:
            results.append((cid, ""))
            continue
        
        # Dipnot div'i
        fn_div = div.find("div", style=lambda s: s and "font-size:12px" in s)
        if fn_div:
            # Aciklama.php linki var mı?
            link = fn_div.find("a", href=lambda h: h and "Aciklama.php" in h)
            has_more = (link is not None) and len(link.get_text().strip()) > 0
            
            if not has_more:
                # Kısa açıklama, doğrudan siteden al
                fn_soup = BeautifulSoup(str(fn_div), "html.parser")
                for a in fn_soup.find_all("a"):
                    a.decompose()
                text = fn_soup.get_text().strip()
                # Temizle
                text = re.sub(r'\s+', ' ', text).strip()
                results.append((cid, text))
            else:
                # Uzun açıklama, veritabanına bak
                existing = get_existing_explanation(db_path, ayet_id, cid)
                if existing:
                    results.append((cid, existing))
                else:
                    # Siteden Aciklama.php'yi çek
                    ac_url = f"{BASE_URL}Aciklama.php?meal={cid}&sureno={sure_no}&ayet={ayet_no}"
                    ac_html = fetch_url(ac_url)
                    full_text = parse_aciklama_page(ac_html)
                    results.append((cid, full_text))
        else:
            results.append((cid, ""))
            
    return ayet_id, sure_no, ayet_no, results

def get_processed_ayets():
    if not os.path.exists(RESUME_FILE):
        return set()
    processed = set()
    with open(RESUME_FILE, "r", encoding="utf-8") as f:
        for line in f:
            parts = line.strip().split(",")
            if len(parts) == 2:
                processed.add((int(parts[0]), int(parts[1])))
    return processed

def mark_processed(sure_no, ayet_no):
    with open(RESUME_FILE, "a", encoding="utf-8") as f:
        f.write(f"{sure_no},{ayet_no}\n")

def main():
    # Adım 1: Veritabanı bağlantısı
    conn = sqlite3.connect(DB_PATH)
    c = conn.cursor()
    
    # MealAciklamalari tablosunu kontrol et
    c.execute("""
    CREATE TABLE IF NOT EXISTS MealAciklamalari (
        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
        AyetId         INTEGER NOT NULL,
        CevirmenId     TEXT    NOT NULL,
        YazarAdi       TEXT,
        AciklamaMetni  TEXT,
        FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
    )
    """)
    # Unique index oluştur (AyetId, CevirmenId için)
    c.execute("CREATE UNIQUE INDEX IF NOT EXISTS idx_aciklama_unique ON MealAciklamalari(AyetId, CevirmenId)")
    conn.commit()

    # Tüm ayetleri al
    c.execute("""
        SELECT A.Id, S.SureNo, A.AyetNo 
        FROM Ayetler A 
        JOIN Sureler S ON A.SureId = S.Id 
        ORDER BY S.SureNo, A.AyetNo
    """)
    all_verses = c.fetchall()
    conn.close()

    processed = get_processed_ayets()
    pending_verses = [v for v in all_verses if (v[1], v[2]) not in processed]

    total = len(all_verses)
    done_count = total - len(pending_verses)
    print(f"Toplam Ayet Sayısı  : {total}")
    print(f"Tamamlanan          : {done_count}")
    print(f"Bekleyen            : {len(pending_verses)}")

    if not pending_verses:
        print("Tüm açıklamalar başarıyla işlendi!")
    else:
        print("8 thread ile çekim işlemi başlatılıyor...")
        
        # Batch yazma için liste
        batch_size = 50
        batch_data = []

        try:
            with ThreadPoolExecutor(max_workers=8) as executor:
                future_to_verse = {
                    executor.submit(process_verse, DB_PATH, ayet_id, sure_no, ayet_no): (ayet_id, sure_no, ayet_no)
                    for ayet_id, sure_no, ayet_no in pending_verses
                }
                
                for future in as_completed(future_to_verse):
                    ayet_id, sure_no, ayet_no = future_to_verse[future]
                    try:
                        _, _, _, results = future.result()
                        done_count += 1
                        pct = (done_count / total) * 100
                        
                        # Eşleşen açıklamaları listeye ekle
                        # results: [(cevirmen_id, aciklama_metni)]
                        for cid, text in results:
                            batch_data.append((ayet_id, cid, ID_TO_YAZAR[cid], text))
                            
                        # Batch dolduğunda veya son kayıtta veritabanına yaz
                        if len(batch_data) >= batch_size * len(ID_TO_YAZAR) or done_count == total:
                            conn_write = sqlite3.connect(DB_PATH)
                            c_write = conn_write.cursor()
                            c_write.executemany("""
                                INSERT OR REPLACE INTO MealAciklamalari (AyetId, CevirmenId, YazarAdi, AciklamaMetni)
                                VALUES (?, ?, ?, ?)
                            """, batch_data)
                            conn_write.commit()
                            conn_write.close()
                            batch_data.clear()
                            
                        mark_processed(sure_no, ayet_no)
                        print(f"  [{pct:6.2f}%] Sure {sure_no:3d} Ayet {ayet_no:3d} işlendi. ({done_count}/{total})")
                    except Exception as e:
                        print(f"\n[HATA] Sure {sure_no} Ayet {ayet_no} işlenirken hata oluştu: {e}")
                        print("Script durduruluyor, kaldığı yerden devam edebilirsiniz.")
                        raise e
        except KeyboardInterrupt:
            print("\n[DURDURULDU] Kullanıcı durdurdu. Kaldığı yerden devam edebilirsin.")
            return

    # Adım 3: NuzulSebepleri tablosunu yeniden yapılandır
    print("\n--- NuzulSebepleri Tablosu Yeniden Yapılandırılıyor ---")
    conn = sqlite3.connect(DB_PATH)
    c = conn.cursor()
    
    # NuzulSebepleri tablosunu temizle
    print("Mevcut nüzul tablosu temizleniyor...")
    c.execute("DELETE FROM NuzulSebepleri")
    
    # MealAciklamalari tablosundaki tüm boş olmayan açıklamaları tara
    print("Açıklamalar içinden nüzul sebepleri taranıyor...")
    c.execute("SELECT AyetId, AciklamaMetni FROM MealAciklamalari WHERE AciklamaMetni != ''")
    rows = c.fetchall()
    
    nuzul_count = 0
    keywords = ["nuzul", "nazil", "iniş sebebi", "esbab", "sebebiyle in", "münasebeti", "nüzul", "nâzil"]
    
    # Aynı ayete ait mükerrer nüzul metinlerini önlemek için set kullanalım
    inserted_pairs = set()

    for ayet_id, text in rows:
        text_lower = text.lower()
        if any(kw in text_lower for kw in keywords):
            # Ayetteki dipnot numaralarını ve paragrafları temizle
            clean_text = text.strip()
            if (ayet_id, clean_text) not in inserted_pairs:
                c.execute("""
                    INSERT INTO NuzulSebepleri (AyetId, Aciklama)
                    VALUES (?, ?)
                """, (ayet_id, clean_text))
                inserted_pairs.add((ayet_id, clean_text))
                nuzul_count += 1
                
    conn.commit()
    print(f"Toplam {nuzul_count} adet temiz ve tam nüzul sebebi başarıyla kaydedildi.")
    
    # Son istatistikler
    print("\n=== Veritabanı Son Durum ===")
    c.execute("SELECT COUNT(*) FROM MealAciklamalari")
    total_ac = c.fetchone()[0]
    c.execute("SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni != ''")
    filled_ac = c.fetchone()[0]
    c.execute("SELECT COUNT(*) FROM NuzulSebepleri")
    total_nz = c.fetchone()[0]
    print(f"MealAciklamalari Toplam Satır   : {total_ac:,}")
    print(f"Açıklama Bulunan Satır Sayısı  : {filled_ac:,}")
    print(f"NuzulSebepleri Satır Sayısı    : {total_nz:,}")
    print("=============================")
    
    conn.close()

if __name__ == "__main__":
    main()
