using KuranMealApp.Models;

namespace KuranMealApp.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Sure>> GetSurelerAsync(bool sortByChronology = false);
    Task<List<Ayet>> GetAyetlerAsync(int sureId);
    Task<List<MealWithAciklama>> GetMeallerForAyetAsync(int ayetId, List<string>? selectedTranslators = null);
    Task<List<string>> GetAvailableTranslatorsAsync();
    Task<List<NuzulSebebi>> GetNuzulSebepleriForAyetAsync(int ayetId);
    Task<Sure?> GetSureByNoAsync(int sureNo);
    Task<List<AramaSonucu>> SearchAyetlerAsync(string query, List<string> selectedTranslators);
}
