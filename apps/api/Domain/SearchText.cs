using System.Globalization;
using System.Text;

namespace RecipEBook.Api.Domain;

/// <summary>
/// Builds a case- and accent-insensitive form of a text for searching:
/// "Soupe de LÉGUMES" becomes "soupe de legumes".
/// SQLite only folds the case of ASCII letters, so the search is done on this form.
/// </summary>
internal static class SearchText
{
    public static string Normalize(string value)
    {
        // FormD splits each accented letter into its base letter followed by
        // combining marks: "é" becomes "e" + U+0301 (combining acute accent).
        string decomposed = value.Normalize(NormalizationForm.FormD);

        StringBuilder builder = new(decomposed.Length);
        foreach (char c in decomposed)
        {
            // Diacritics are nonspacing combining characters (Unicode category "Mn").
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().ToLowerInvariant();
    }
}
