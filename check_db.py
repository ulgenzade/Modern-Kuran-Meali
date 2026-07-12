import sqlite3, os

conn = sqlite3.connect('kuran.db')
c = conn.cursor()

print('=== kuran.db Mevcut Durum ===')

tables = ['Sureler', 'Ayetler', 'Mealler', 'NuzulSebepleri', 'MealAciklamalari']
for t in tables:
    try:
        c.execute(f'SELECT COUNT(*) FROM {t}')
        cnt = c.fetchone()[0]
        print(f'{t}: {cnt:,} satir')
    except Exception as e:
        print(f'{t}: TABLO YOK - {e}')

print()

# Mealler kolonlari
try:
    c.execute('PRAGMA table_info(Mealler)')
    cols = [row[1] for row in c.fetchall()]
    print('Mealler kolonlari:', cols)
except Exception as e:
    print('Kolon kontrol hatasi:', e)

print()

# Kac cevirmen var?
try:
    c.execute('SELECT COUNT(DISTINCT YazarAdi) FROM Mealler')
    print('Farkli cevirmen sayisi:', c.fetchone()[0])
    c.execute('SELECT DISTINCT YazarAdi FROM Mealler ORDER BY YazarAdi')
    yazarlar = [r[0] for r in c.fetchall()]
    print('Cevirmenler:')
    for y in yazarlar:
        print('  -', y)
except Exception as e:
    print('Cevirmen sorgu hatasi:', e)

print()
size_mb = os.path.getsize('kuran.db') / (1024*1024)
print('DB boyutu: %.1f MB' % size_mb)
conn.close()
