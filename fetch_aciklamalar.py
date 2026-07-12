"""
fetch_aciklamalar.py
====================
Mevcut kuran.db'ye MealAciklamalari tablosunu ekler ve
kuranmeali.com/Aciklama.php adresinden her cevirmenin
her ayetteki aciklama metnini ceker.

Strateji:
  - YazarAdi -> CevirmenId eslemesi yerine
    Ayetler tablosundaki (SureNo, AyetNo) ciftleri alinir,
    sonra 54 cevirmenin hepsi icin denenir.
  - Zaten islenmis olanlar atlanir (resume destegi).

Kullanim:
  python fetch_aciklamalar.py                # Tum Kuran (3 thread)
  python fetch_aciklamalar.py --threads 4
  python fetch_aciklamalar.py --test         # Sadece Fatiha (1. sure)
  python fetch_aciklamalar.py --db baska.db
"""

import re
import sys
import time
import random
import sqlite3
import argparse
import threading
from urllib.parse import urljoin
from concurrent.futures import ThreadPoolExecutor, as_completed

import requests
from bs4 import BeautifulSoup

BASE_URL = "https://www.kuranmeali.com/"

# Sitedeki 54 cevirmenin ID listesi (Aciklama.php ?meal= parametresi)
CEVIRMEN_IDS = [
    "golpinarli", "aakgul", "aparliyan", "ahmettekin", "ahmetvarol",
    "abulac", "alifikriyavuz", "bahaeddinsaglam", "bayraktar", "besimatalay",
    "cemalkulunkoglu", "cemilsaid", "diyanetislerieski", "diyanetisleriyeni",
    "kuranyolu", "diyanetvakfi", "elmalilisade", "elmaliliorj", "demiryent",
    "erhanaktas", "hbasricantay", "haydarozturkserkanyilmaz", "hayrat",
    "ihsanaktas", "ilyasyorulmaz", "baltacioglu", "ismailhakkiizmirli",
    "ismailyakit", "kadricelik", "mahmutkisa", "mahmutozdemir", "mehmetcakir",
    "mehmetcoban", "mehmetokuyan", "mehmetturk", "muhammedesed", "mustafacavdar",
    "islamoglu", "orhankuntman", "osmanfirat", "omernasuhi", "syildirim",
    "sates", "suleymantevfik", "abayindir", "spiris", "simsek", "ynuri",
    "sardorxonjahongir", "eskianadoluturkcesi", "satiralti", "bunyadov",
    "pickthall", "yusufali",
]

USER_AGENTS = [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 Chrome/124 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36 Edg/124",
]

_thread_local = threading.local()
_db_lock = threading.Lock()


# ---------------------------------------------------------------------------
# HTTP
# ---------------------------------------------------------------------------

def get_session():
    if not hasattr(_thread_local, "sess"):
        s = requests.Session()
        s.headers.update({
            "Accept": "text/html,*/*;q=0.8",
            "Accept-Language": "tr,en;q=0.5",
        })
        _thread_local.sess = s
    return _thread_local.sess


def fetch_url(url, retries=5, backoff=2):
    session = get_session()
    for attempt in range(retries):
        try:
            time.sleep(random.uniform(0.1, 0.4))
            r = session.get(url, headers={"User-Agent": random.choice(USER_AGENTS)}, timeout=18)
            if r.status_code == 200:
                r.encoding = "utf-8"
                return r.text
            elif r.status_code in (429, 500, 502, 503, 504):
                wait = backoff ** attempt + random.uniform(1, 3)
                print("  [HTTP %d] %.1fs: %s" % (r.status_code, wait, url))
                time.sleep(wait)
            else:
                return None  # 404 = aciklama yok
        except requests.exceptions.Timeout:
            wait = backoff ** attempt + random.uniform(1, 2)
            print("  [Timeout] deneme %d/%d: %s" % (attempt + 1, retries, url))
            time.sleep(wait)
        except Exception as exc:
            wait = backoff ** attempt + random.uniform(0.5, 1.5)
            print("  [Err] %s (%d/%d): %s" % (exc, attempt + 1, retries, url))
            time.sleep(wait)
    return None


# ---------------------------------------------------------------------------
# Aciklama Parser
# ---------------------------------------------------------------------------

def parse_aciklama(html):
    """Aciklama.php HTML'inden tam aciklama metnini ayiklar. Bos sayfalarda bos string."""
    if not html:
        return ""

    soup = BeautifulSoup(html, "html.parser")
    paragraphs = []

    for td in soup.find_all("td"):
        # Baslik satiri: bold + font etiketi iceriyor
        if td.find("b") and td.find("font"):
            continue
        # Kapat butonu
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


def fetch_aciklama_for(cevirmen_id, sure_no, ayet_no):
    """Aciklama.php'den tek bir (cevirmen, ayet) icin metin ceker. Doner (success, text)"""
    url = urljoin(
        BASE_URL,
        "Aciklama.php?meal=%s&sureno=%d&ayet=%d" % (cevirmen_id, sure_no, ayet_no)
    )
    html = fetch_url(url)
    if html is None:
        return False, None
    return True, parse_aciklama(html)


# ---------------------------------------------------------------------------
# DB Setup
# ---------------------------------------------------------------------------

def setup_table(conn):
    conn.execute("""
    CREATE TABLE IF NOT EXISTS MealAciklamalari (
        Id             INTEGER PRIMARY KEY AUTOINCREMENT,
        AyetId         INTEGER NOT NULL,
        CevirmenId     TEXT    NOT NULL,
        AciklamaMetni  TEXT,
        FOREIGN KEY(AyetId) REFERENCES Ayetler(Id) ON DELETE CASCADE
    )
    """)
    conn.execute("""
    CREATE UNIQUE INDEX IF NOT EXISTS idx_ac_unique
    ON MealAciklamalari(AyetId, CevirmenId)
    """)
    conn.commit()
    print("[DB] MealAciklamalari tablosu hazir.")


def get_pending_jobs(conn, limit_to_fatiha=False):
    """
    (AyetId, CevirmenId, SureNo, AyetNo) listesini doner.
    Zaten islenmis olanlar dahil edilmez (resume destegi).
    """
    sure_filter = "AND S.SureNo = 1" if limit_to_fatiha else ""
    return conn.execute("""
        SELECT MA.AyetId, MA.CevirmenId, S.SureNo, A.AyetNo
        FROM   MealAciklamalari MA
        JOIN   Ayetler A ON MA.AyetId = A.Id
        JOIN   Sureler S ON A.SureId = S.Id
        WHERE  MA.AciklamaMetni IS NULL
        %s
        ORDER  BY S.SureNo, A.AyetNo, MA.CevirmenId
    """ % sure_filter).fetchall()


def save_result(conn, ayet_id, cevirmen_id, metin):
    with _db_lock:
        try:
            conn.execute("""
            INSERT OR REPLACE INTO MealAciklamalari (AyetId, CevirmenId, AciklamaMetni)
            VALUES (?, ?, ?)
            """, (ayet_id, cevirmen_id, metin))
            conn.commit()
        except Exception as exc:
            print("  [DB Hata] AyetId=%d [%s]: %s" % (ayet_id, cevirmen_id, exc))


def worker(conn, ayet_id, cevirmen_id, sure_no, ayet_no):
    success, metin = fetch_aciklama_for(cevirmen_id, sure_no, ayet_no)
    if success:
        save_result(conn, ayet_id, cevirmen_id, metin or "")
        return True
    return False


# ---------------------------------------------------------------------------
# Stats
# ---------------------------------------------------------------------------

def print_stats(conn):
    print("\n=== Veritabani Durumu ===")
    for tbl in ["Sureler", "Ayetler", "Mealler", "MealAciklamalari", "NuzulSebepleri"]:
        try:
            cnt = conn.execute("SELECT COUNT(*) FROM %s" % tbl).fetchone()[0]
        except Exception:
            cnt = "YOK"
        print("  %-30s: %s" % (tbl, ("{:,}".format(cnt) if isinstance(cnt, int) else cnt)))

    try:
        with_ac = conn.execute(
            "SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni != ''"
        ).fetchone()[0]
        total_ac = conn.execute(
            "SELECT COUNT(*) FROM MealAciklamalari"
        ).fetchone()[0]
        print("")
        print("  Aciklama BULUNAN : {:,}".format(with_ac))
        print("  Aciklama YOK     : {:,}".format(total_ac - with_ac))
        print("  Toplam islenen   : {:,}".format(total_ac))
    except Exception:
        pass
    print("=" * 42)


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Aciklama Cekici")
    parser.add_argument("--db",      default="kuran.db")
    parser.add_argument("--threads", type=int, default=3)
    parser.add_argument("--test",    action="store_true", help="Sadece Fatiha")
    args = parser.parse_args()

    print("=" * 55)
    print("  KuranMeali Aciklama Cekici")
    print("  Veritabani : %s" % args.db)
    print("  Thread     : %d" % args.threads)
    print("  Mod        : %s" % ("TEST - Sadece Fatiha" if args.test else "TUM KURAN"))
    print("=" * 55)

    conn = sqlite3.connect(args.db, check_same_thread=False)
    conn.execute("PRAGMA journal_mode = WAL")
    conn.execute("PRAGMA synchronous  = NORMAL")
    conn.execute("PRAGMA cache_size   = -65536")
    conn.execute("PRAGMA foreign_keys = ON")

    setup_table(conn)

    print("\n[Tarama] Eksik isler hesaplaniyor...")
    jobs  = get_pending_jobs(conn, limit_to_fatiha=args.test)
    total = len(jobs)

    if not jobs:
        print("[TAMAM] Hic eksik is yok!")
        print_stats(conn)
        conn.close()
        return

    print("[Bekleyen] {:,} islem bulundu ({} ayet x {} cevirmen)".format(
        total,
        total // len(CEVIRMEN_IDS) if not args.test else 7,
        len(CEVIRMEN_IDS)
    ))
    print("[Basliyor] %d thread ile basliyor..." % args.threads)
    print("  VAR -> kaydedilir | YOK -> bos isaretlenir (bir daha denenmez)\n")

    found = 0
    done  = 0

    try:
        with ThreadPoolExecutor(max_workers=args.threads) as executor:
            future_map = {
                executor.submit(worker, conn, ayet_id, cid, sure_no, ayet_no): (ayet_id, cid, sure_no, ayet_no)
                for ayet_id, cid, sure_no, ayet_no in jobs
            }

            for future in as_completed(future_map):
                ayet_id, cid, sure_no, ayet_no = future_map[future]
                done += 1
                try:
                    ok = future.result()
                    if ok:
                        found += 1
                        pct = done / total * 100
                        print("  [%6.2f%%] ACIKLAMA Sure%3d Ayet%3d [%-26s] (%s/%s, %d bulundu)" % (
                            pct, sure_no, ayet_no, cid,
                            "{:,}".format(done), "{:,}".format(total), found
                        ))
                    elif done % 500 == 0 or done == total:
                        pct = done / total * 100
                        print("  [%6.2f%%] Isleniyor... (%s/%s, %d aciklama)" % (
                            pct, "{:,}".format(done), "{:,}".format(total), found
                        ))
                except Exception as exc:
                    print("  [HATA] Sure%d Ayet%d [%s]: %s" % (sure_no, ayet_no, cid, exc))

    except KeyboardInterrupt:
        print("\n[DURDURULDU] Kaldigi yerden devam edebilirsin.")

    print("\n[TAMAMLANDI] %s aciklama / %s islem" % (
        "{:,}".format(found), "{:,}".format(done)
    ))
    print_stats(conn)
    conn.close()


if __name__ == "__main__":
    main()
