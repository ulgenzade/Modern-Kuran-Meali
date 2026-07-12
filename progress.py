import sqlite3
import os
import sys

db_name = 'kuran_new.db' if os.path.exists('kuran_new.db') else 'kuran.db'
if not os.path.exists(db_name):
    print(f"Hata: Veritabanı bulunamadı ({db_name})")
    sys.exit(1)

conn = sqlite3.connect(db_name)
cursor = conn.cursor()

print(f"=== {db_name} İlerleme Durumu ===")

# Sure ve Ayet İstatistikleri
try:
    sure_cnt = cursor.execute('SELECT COUNT(*) FROM Sureler').fetchone()[0]
    ayet_cnt = cursor.execute('SELECT COUNT(*) FROM Ayetler').fetchone()[0]
    print(f"Sureler    : {sure_cnt:3d} / 114")
    print(f"Ayetler    : {ayet_cnt:,} / 6,236")
except Exception as e:
    print(f"Ayet bilgisi alınamadı: {e}")

# Mealler İstatistikleri
try:
    meal_cnt = cursor.execute('SELECT COUNT(*) FROM Mealler').fetchone()[0]
    print(f"Mealler    : {meal_cnt:,} / 336,744")
except Exception as e:
    print(f"Meal bilgisi alınamadı: {e}")

# Açıklamalar İstatistikleri
try:
    total_expected = 336744
    
    # Yeni modeldeki durumları çek
    completed = cursor.execute('SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni IS NOT NULL').fetchone()[0]
    pending = cursor.execute('SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni IS NULL').fetchone()[0]
    found = cursor.execute('SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni IS NOT NULL AND AciklamaMetni != ""').fetchone()[0]
    empty = cursor.execute('SELECT COUNT(*) FROM MealAciklamalari WHERE AciklamaMetni = ""').fetchone()[0]
    
    pct = (completed / total_expected) * 100
    
    print("\n--- Açıklama (Tefsir) Durumu ---")
    print(f"Tamamlanan : {completed:,} / {total_expected:,} ({pct:.2f}%)")
    print(f"Bekleyen   : {pending:,}")
    print(f"Bulunan    : {found:,} (Tefsir içeren)")
    print(f"Boş (Yok)  : {empty:,} (Sitede açıklama bulunmayan)")
    
    if pending > 0:
        # Kalan işlerin tahmini süre hesabı (ortalama 3 thread ile saniyede ~10 istek yapıldığı varsayılırsa)
        tahmini_sn = pending / 10
        dakika = int(tahmini_sn // 60)
        saniye = int(tahmini_sn % 60)
        print(f"Tahmini Süre: ~ {dakika} dk {saniye} sn (saniyede ~10 sayfa hızıyla)")
    else:
        print("\n*** AÇIKLAMALAR TAMAMEN BİTTİ! ***")
        
    # Son çekilen 5 açıklamayı listele (hareketlilik görmek için)
    print("\n--- Son Güncellenen 5 Açıklama ---")
    cursor.execute("""
        SELECT MA.YazarAdi, S.SureAdi, A.AyetNo, SUBSTR(MA.AciklamaMetni, 1, 80)
        FROM MealAciklamalari MA
        JOIN Ayetler A ON MA.AyetId = A.Id
        JOIN Sureler S ON A.SureId = S.Id
        WHERE MA.AciklamaMetni IS NOT NULL AND MA.AciklamaMetni != ""
        ORDER BY MA.Id DESC
        LIMIT 5
    """)
    recent = cursor.fetchall()
    for yazar, sure, ayet, text in recent:
        clean_text = text.replace('\n', ' ').strip()
        print(f"  [{sure} {ayet}. Ayet] {yazar}: {clean_text}...")

except Exception as e:
    print(f"Açıklama bilgisi alınamadı: {e}")

# Nüzul Sebebi İstatistikleri
try:
    nuzul_cnt = cursor.execute('SELECT COUNT(*) FROM NuzulSebepleri').fetchone()[0]
    print(f"\nNüzul Seb. : {nuzul_cnt:,} adet")
except Exception as e:
    pass

conn.close()
