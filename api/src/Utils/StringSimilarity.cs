using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace api_bora_trampar.src.Utils
{
    public static class StringSimilarity
    {
        public static string NormalizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string clean = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            clean = Regex.Replace(clean, @"[^\w\s]", " ");
            clean = Regex.Replace(clean, @"\s+", " ").Trim();

            return clean;
        }

        private static string StemWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;
            string w = word.Trim().ToLowerInvariant();

            if (w.Length > 4 && w.EndsWith('s'))
            {
                return w.Substring(0, w.Length - 1);
            }
            return w;
        }

        public static HashSet<string> ExtractKeywords(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new HashSet<string>();

            string clean = NormalizeText(input);

            clean = Regex.Replace(clean, @"\d+", " ");

            clean = Regex.Replace(clean, @"\s+", " ").Trim();

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "de", "da", "do", "das", "dos", "e", "ou", "em", "para", "com", "sem", "por", "sobre", "sob", "ao", "aos", "na", "no", "nas", "nos"
            };

            var words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Select(StemWord);

            return new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
        }

        public static string StripNumbersAndCodes(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            string clean = NormalizeText(input);
            clean = Regex.Replace(clean, @"\d+", " ");
            return Regex.Replace(clean, @"\s+", " ").Trim();
        }

        public static double CalculateSimilarity(string? s1, string? s2)
        {
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2)) return 0.0;

            string norm1 = NormalizeText(s1);
            string norm2 = NormalizeText(s2);

            if (norm1 == norm2) return 1.0;

            string textOnly1 = StripNumbersAndCodes(s1);
            string textOnly2 = StripNumbersAndCodes(s2);

            if (!string.IsNullOrEmpty(textOnly1) && textOnly1 == textOnly2) return 1.0;

            double substringScore = 0.0;

            if (!string.IsNullOrEmpty(textOnly1) && !string.IsNullOrEmpty(textOnly2))
            {
                if (textOnly1.Contains(textOnly2) || textOnly2.Contains(textOnly1))
                {
                    int minL = Math.Min(textOnly1.Length, textOnly2.Length);
                    int maxL = Math.Max(textOnly1.Length, textOnly2.Length);
                    substringScore = 0.80 + (0.20 * ((double)minL / maxL));
                }
            }

            var kw1 = ExtractKeywords(s1);
            var kw2 = ExtractKeywords(s2);

            double tokenScore = 0.0;
            if (kw1.Count > 0 && kw2.Count > 0)
            {
                int intersectionCount = kw1.Count(w1 => kw2.Any(w2 =>
                    w1 == w2 ||
                    (w1.Length >= 4 && w2.Length >= 4 && (w1.StartsWith(w2) || w2.StartsWith(w1)))
                ));

                int minCount = Math.Min(kw1.Count, kw2.Count);
                int unionCount = kw1.Union(kw2).Count();

                double overlapMin = (double)intersectionCount / minCount;
                double jaccard = unionCount > 0 ? (double)intersectionCount / unionCount : 0.0;

                bool hasSignificantTerm = kw1.Any(w1 => w1.Length >= 4 && kw2.Any(w2 =>
                    w1 == w2 || (w1.Length >= 4 && w2.Length >= 4 && (w1.StartsWith(w2) || w2.StartsWith(w1)))));

                if (hasSignificantTerm && intersectionCount >= 1)
                {

                    tokenScore = 0.60 + (0.40 * overlapMin);
                }
                else
                {
                    tokenScore = (overlapMin * 0.7) + (jaccard * 0.3);
                }
            }

            int distance = LevenshteinDistance(textOnly1, textOnly2);
            int maxLen = Math.Max(textOnly1.Length, textOnly2.Length);
            double levScore = maxLen > 0 ? 1.0 - ((double)distance / maxLen) : 0.0;

            return Math.Max(substringScore, Math.Max(tokenScore, levScore));
        }

        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[n, m];
        }
    }
}
