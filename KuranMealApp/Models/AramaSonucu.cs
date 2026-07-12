using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace KuranMealApp.Models
{
    [Preserve(AllMembers = true)]
    public class AramaSonucu
    {
        public int AyetId { get; set; }
        public int SureNo { get; set; }
        public string SureAdi { get; set; } = string.Empty;
        public int AyetNo { get; set; }
        public string ArapcaMetin { get; set; } = string.Empty;
        public string MealMetni { get; set; } = string.Empty;
        public string YazarAdi { get; set; } = string.Empty;

        // UI için yardımcı özellik
        public string GosterimBasligi => $"{SureAdi} Suresi, {AyetNo}. Ayet";

        // Vurgulama için kullanılacak arama terimi
        public string SearchQuery { get; set; } = string.Empty;

        public FormattedString FormattedMealMetni => GenerateFormattedString(MealMetni, SearchQuery);
        public FormattedString FormattedArapcaMetin => GenerateFormattedString(ArapcaMetin, SearchQuery);

        private FormattedString GenerateFormattedString(string text, string query)
        {
            var formattedString = new FormattedString();

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                formattedString.Spans.Add(new Span { Text = text });
                return formattedString;
            }

            // Regex ile büyük/küçük harf duyarsız arama
            string pattern = Regex.Escape(query);
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

            if (matches.Count == 0)
            {
                formattedString.Spans.Add(new Span { Text = text });
                return formattedString;
            }

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                // Eşleşmeden önceki metin
                if (match.Index > lastIndex)
                {
                    formattedString.Spans.Add(new Span { Text = text.Substring(lastIndex, match.Index - lastIndex) });
                }

                // Eşleşen vurgulu metin
                formattedString.Spans.Add(new Span 
                { 
                    Text = match.Value, 
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Red // Veya temanıza uygun vurgu rengi
                });

                lastIndex = match.Index + match.Length;
            }

            // Eşleşmeden sonra kalan metin
            if (lastIndex < text.Length)
            {
                formattedString.Spans.Add(new Span { Text = text.Substring(lastIndex) });
            }

            return formattedString;
        }
    }
}