using SQLite;
using KuranMealApp.Models;

namespace KuranMealApp.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;
    private const long MinExpectedDbSize = 130_000_000;

    // ─── YazarAdi → CevirmenId mapping ───────────────────────────────────
    // Mealler tablosundaki YazarAdi değerlerini MealAciklamalari'ndaki
    // CevirmenId değerlerine eşler. Bu olmadan açıklamalar JOIN yapamaz.
    private static readonly Dictionary<string, string> YazarToId = new()
    {
        ["Abdulbaki Gölpınarlı Meali"]        = "golpinarli",
        ["Abdullah Parlıyan Meali"]            = "aparliyan",
        ["Abdullah-Ahmet Akgül Meali"]         = "aakgul",
        ["Ahmet Tekin Meali"]                  = "ahmettekin",
        ["Ahmet Varol Meali"]                  = "ahmetvarol",
        ["Ali Bulaç Meali"]                    = "abulac",
        ["Ali Fikri Yavuz Meali"]              = "alifikriyavuz",
        ["Bahaeddin Sağlam Meali"]             = "bahaeddinsaglam",
        ["Bayraktar Bayraklı Meali"]           = "bayraktar",
        ["Besim Atalay Meali (1965)"]          = "besimatalay",
        ["Bunyadov-Memmedeliyev"]              = "bunyadov",
        ["Cemal Külünkoğlu Meali"]             = "cemalkulunkoglu",
        ["Cemil Said (1924)"]                  = "cemilsaid",
        ["Diyanet Vakfı Meali"]                = "diyanetvakfi",
        ["Diyanet İşleri Meali (Eski)"]        = "diyanetislerieski",
        ["Diyanet İşleri Meali (Yeni)"]        = "diyanetisleriyeni",
        ["Elmalılı Hamdi Yazır Meali"]         = "elmalilisade",
        ["Elmalılı Meali (Orijinal)"]          = "elmaliliorj",
        ["Emrah Demiryent Meali"]              = "demiryent",
        ["Erhan Aktaş Meali"]                  = "erhanaktas",
        ["Eski Anadolu Türkçesi"]              = "eskianadoluturkcesi",
        ["Hasan Basri Çantay Meali"]           = "hbasricantay",
        ["Haydar Öztürk-Serkan Yılmaz Meali"] = "haydarozturkserkanyilmaz",
        ["Hayrat Neşriyat Meali"]              = "hayrat",
        ["İhsan Aktaş Meali"]                  = "ihsanaktas",
        ["İlyas Yorulmaz Meali"]               = "ilyasyorulmaz",
        ["İsmayıl Hakkı Baltacıoğlu"]          = "baltacioglu",
        ["İsmail Hakkı İzmirli"]               = "ismailhakkiizmirli",
        ["İsmail Yakıt"]                       = "ismailyakit",
        ["Kadri Çelik Meali"]                  = "kadricelik",
        ["Kur'an Yolu (Diyanet İşleri)"]       = "kuranyolu",
        ["M. Pickthall (English)"]             = "pickthall",
        ["Mahmut Kısa Meali"]                  = "mahmutkisa",
        ["Mahmut Özdemir Meali"]               = "mahmutozdemir",
        ["Mehmet Çakır Meali"]                 = "mehmetcakir",
        ["Mehmet Çoban Meali"]                 = "mehmetcoban",
        ["Mehmet Okuyan Meali"]                = "mehmetokuyan",
        ["Mehmet Türk Meali"]                  = "mehmetturk",
        ["Muhammed Esed Meali"]                = "muhammedesed",
        ["Mustafa Çavdar Meali"]               = "mustafacavdar",
        ["Mustafa İslamoğlu Meali"]            = "islamoglu",
        ["Orhan Kuntman Meali"]                = "orhankuntman",
        ["Osman Fırat Meali"]                  = "osmanfirat",
        ["Sardorxon Jahongir"]                 = "sardorxonjahongir",
        ["Satıraltı Meal (1534)"]              = "satiralti",
        ["Suat Yıldırım Meali"]                = "syildirim",
        ["Süleyman Ateş Meali"]                = "sates",
        ["Süleyman Tevfik (1927)"]             = "suleymantevfik",
        ["Süleymaniye Vakfı Meali"]            = "abayindir",
        ["Şaban Piriş Meali"]                  = "spiris",
        ["Ümit Şimşek Meali"]                  = "simsek",
        ["Yaşar Nuri Öztürk Meali"]            = "ynuri",
        ["Yusuf Ali (English)"]                = "yusufali",
        ["Ömer Nasuhi Bilmen Meali"]           = "omernasuhi",
    };

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "kuran.db");
    }

    private static readonly SemaphoreSlim _initSemaphore = new(1, 1);

    public async Task InitializeAsync()
    {
        if (_connection != null) return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_connection != null) return;

            const int CurrentDbVersion = 5;
            int installedVersion = Microsoft.Maui.Storage.Preferences.Default.Get("DbVersion", 0);

            bool needsCopy = !File.Exists(_dbPath) 
                             || new FileInfo(_dbPath).Length < MinExpectedDbSize
                             || installedVersion < CurrentDbVersion;

            if (needsCopy)
            {
                if (File.Exists(_dbPath))
                {
                    try { File.Delete(_dbPath); } catch { }
                }
                bool copied = await TryCopyBundledDatabase();
                if (copied)
                {
                    Microsoft.Maui.Storage.Preferences.Default.Set("DbVersion", CurrentDbVersion);
                }
                else
                {
                    await CreateMockDatabaseAsync();
                }
            }

            _connection = new SQLiteAsyncConnection(_dbPath);
            await _connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await _connection.ExecuteAsync("PRAGMA cache_size = -32768;");
            System.Diagnostics.Debug.WriteLine($"[DB] Bağlandı. Sure: {await _connection.Table<Sure>().CountAsync()}");
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    private async Task<bool> TryCopyBundledDatabase()
    {
        try
        {
#if ANDROID
            await Task.Run(async () =>
            {
                var context = Android.App.Application.Context;
                using var assetStream = context.Assets.Open("kuran.gz");
                using var gzipStream = new System.IO.Compression.GZipStream(assetStream, System.IO.Compression.CompressionMode.Decompress);
                using var newStream = new FileStream(_dbPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await gzipStream.CopyToAsync(newStream);
            });
            return true;
#else
            using var stream = await FileSystem.OpenAppPackageFileAsync("kuran.gz");
            await Task.Run(async () => 
            {
                using var gzipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
                using var newStream = new FileStream(_dbPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await gzipStream.CopyToAsync(newStream);
            });
            return true;
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] Kopya hatası: {ex.Message}");
            return false;
        }
    }

    private async Task CreateMockDatabaseAsync()
    {
        var conn = new SQLiteAsyncConnection(_dbPath);
        await conn.CreateTableAsync<Sure>();
        await conn.CreateTableAsync<Ayet>();
        await conn.CreateTableAsync<Meal>();
        await conn.CreateTableAsync<MealAciklama>();
        await conn.CreateTableAsync<NuzulSebebi>();
        var fatiha = new Sure { SureNo = 1, SureAdi = "Fâtiha Suresi (Test)", AyetSayisi = 7, InisSirasi = 5, Tip = "Mekkî" };
        await conn.InsertAsync(fatiha);
        var ayet1 = new Ayet { SureId = fatiha.Id, AyetNo = 1, ArapcaMetin = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ", Transkript = "Bismillâhirrahmânirrahîm" };
        await conn.InsertAsync(ayet1);
        await conn.InsertAsync(new Meal { AyetId = ayet1.Id, YazarAdi = "Diyanet İşleri Meali (Yeni)", MealMetni = "Rahmân ve Rahîm olan Allah'ın adıyla." });
        await conn.CloseAsync();
    }

    private async Task EnsureInitialized()
    {
        if (_connection == null) await InitializeAsync();
    }

    public async Task<List<Sure>> GetSurelerAsync(bool sortByChronology = false)
    {
        await EnsureInitialized();
        return sortByChronology
            ? await _connection!.Table<Sure>().OrderBy(s => s.InisSirasi).ToListAsync()
            : await _connection!.Table<Sure>().OrderBy(s => s.SureNo).ToListAsync();
    }

    public async Task<Sure?> GetSureByNoAsync(int sureNo)
    {
        await EnsureInitialized();
        return await _connection!.Table<Sure>().Where(s => s.SureNo == sureNo).FirstOrDefaultAsync();
    }

    public async Task<List<Ayet>> GetAyetlerAsync(int sureId)
    {
        await EnsureInitialized();
        return await _connection!.Table<Ayet>().Where(a => a.SureId == sureId).OrderBy(a => a.AyetNo).ToListAsync();
    }

    /// <summary>
    /// Mealleri açıklamalarıyla birlikte getirir.
    /// YazarAdi → CevirmenId eşlemesi C# tarafında yapılır çünkü
    /// Mealler tablosundaki CevirmenId kolonu boştur (eski crawler).
    /// </summary>
    public async Task<List<MealWithAciklama>> GetMeallerForAyetAsync(int ayetId, List<string>? selectedTranslators = null)
    {
        await EnsureInitialized();

        var meals = await _connection!.Table<Meal>().Where(m => m.AyetId == ayetId).ToListAsync();
        if (selectedTranslators != null && selectedTranslators.Any())
            meals = meals.Where(m => selectedTranslators.Contains(m.YazarAdi)).ToList();
        if (!meals.Any()) return new List<MealWithAciklama>();

        // CevirmenId'leri YazarAdi'dan türet (DB'de bu kolon boş)
        var mealCidMap = meals.ToDictionary(
            m => m.Id,
            m => !string.IsNullOrEmpty(m.CevirmenId) ? m.CevirmenId
                 : YazarToId.TryGetValue(m.YazarAdi, out var cid) ? cid
                 : null
        );

        var cevirmenIds = mealCidMap.Values.Where(v => v != null).Distinct().Cast<string>().ToList();
        var aciklamaMap = new Dictionary<string, string>();

        if (cevirmenIds.Any())
        {
            var placeholders = string.Join(",", cevirmenIds.Select(_ => "?"));
            var args = new object[] { (object)ayetId }.Concat(cevirmenIds.Cast<object>()).ToArray();
            var aciklamalar = await _connection!.QueryAsync<MealAciklama>(
                $"SELECT CevirmenId, AciklamaMetni FROM MealAciklamalari " +
                $"WHERE AyetId = ? AND CevirmenId IN ({placeholders}) AND AciklamaMetni != ''",
                args
            );
            aciklamaMap = aciklamalar.ToDictionary(a => a.CevirmenId, a => a.AciklamaMetni);
        }

        return meals.Select(m =>
        {
            var cid = mealCidMap.TryGetValue(m.Id, out var v) ? v : null;
            var aciklama = cid != null && aciklamaMap.TryGetValue(cid, out var ac) ? ac : "";
            return new MealWithAciklama(m, aciklama);
        }).ToList();
    }

    public async Task<List<string>> GetAvailableTranslatorsAsync()
    {
        await EnsureInitialized();
        var result = await _connection!.QueryAsync<Meal>("SELECT DISTINCT YazarAdi FROM Mealler ORDER BY YazarAdi");
        return result.Select(r => r.YazarAdi).Where(n => !string.IsNullOrEmpty(n)).ToList();
    }

    public async Task<List<NuzulSebebi>> GetNuzulSebepleriForAyetAsync(int ayetId)
    {
        await EnsureInitialized();
        return await _connection!.Table<NuzulSebebi>().Where(n => n.AyetId == ayetId).ToListAsync();
    }

    public async Task<List<AramaSonucu>> SearchAyetlerAsync(string query, List<string> selectedTranslators)
    {
        await EnsureInitialized();
        if (string.IsNullOrWhiteSpace(query)) return new List<AramaSonucu>();

        string likeQuery = $"%{query}%";

        // Eğer çevirmen seçilmemişse varsayılan olarak Diyanet Yeni kullan.
        if (selectedTranslators == null || !selectedTranslators.Any())
        {
            selectedTranslators = new List<string> { "Diyanet İşleri Meali (Yeni)" };
        }

        var placeholders = string.Join(",", selectedTranslators.Select(_ => "?"));
        var args = new object[] { (object)likeQuery, (object)likeQuery }.Concat(selectedTranslators.Cast<object>()).ToArray();

        string sql = $@"
            SELECT 
                a.Id as AyetId, 
                s.SureNo, 
                s.SureAdi, 
                a.AyetNo, 
                a.ArapcaMetin, 
                m.MealMetni, 
                m.YazarAdi
            FROM Ayetler a
            JOIN Sureler s ON a.SureId = s.Id
            JOIN Mealler m ON m.AyetId = a.Id
            WHERE (m.MealMetni LIKE ? OR a.ArapcaMetin LIKE ?) 
              AND m.YazarAdi IN ({placeholders})
            LIMIT 100";

        var results = await _connection!.QueryAsync<AramaSonucu>(sql, args);
        return results;
    }
}
