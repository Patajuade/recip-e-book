using System.Buffers.Text;
using System.Globalization;
using System.Text;

using Microsoft.Extensions.Localization;

namespace RecipEBook.Api.Common;

/// <summary>One page of a list, and the cursor of the next one (null on the last page).</summary>
public record Page<T>(List<T> Items, string? NextCursor);

/// <summary>
/// Where the next page starts in a list ordered by UpdatedAt then Id, both descending: keyset
/// pagination (EF Core pagination guide). Id breaks ties, so the order is fully unique.
/// </summary>
public sealed record PageCursor(DateTime UpdatedAt, Guid Id)
{
    // Opaque to clients: they send it back as they received it.
    public string Encode() =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes(
            $"{UpdatedAt.Ticks.ToString(CultureInfo.InvariantCulture)}:{Id}"));

    public static PageCursor? Decode(string value)
    {
        try
        {
            string[] parts = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(value)).Split(':');
            return parts.Length == 2
                && long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long ticks)
                && ticks <= DateTime.MaxValue.Ticks
                && Guid.TryParse(parts[1], out Guid id)
                ? new PageCursor(new DateTime(ticks, DateTimeKind.Utc), id)
                : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}

/// <summary>The `limit` and `cursor` query parameters of a paged list, checked.</summary>
public sealed record PageRequest(int Limit, PageCursor? After)
{
    public const int DefaultLimit = 20;

    // Capped so a client can't ask for every row at once.
    public const int MaxLimit = 50;

    public static (PageRequest Request, Dictionary<string, string[]> Errors) From(
        int? limit,
        string? cursor,
        IStringLocalizer<PageRequest> localizer)
    {
        Dictionary<string, string[]> errors = [];

        if (limit is < 1 or > MaxLimit)
        {
            errors["limit"] = [localizer["The limit must be between {0} and {1}.", 1, MaxLimit]];
        }

        PageCursor? after = null;
        if (cursor is not null)
        {
            after = PageCursor.Decode(cursor);
            if (after is null)
            {
                errors["cursor"] = [localizer["The cursor is not valid."]];
            }
        }

        return (new PageRequest(limit ?? DefaultLimit, after), errors);
    }

    /// <summary>
    /// The page from rows fetched with <c>Take(Limit + 1)</c>: an extra row means there is a next
    /// page, which starts after the last row kept.
    /// </summary>
    public Page<TItem> ToPage<TRow, TItem>(
        List<TRow> rows,
        Func<TRow, PageCursor> cursorOf,
        Func<TRow, TItem> toItem)
    {
        bool hasNext = rows.Count > Limit;
        List<TRow> kept = hasNext ? rows[..Limit] : rows;
        return new Page<TItem>(
            [.. kept.Select(toItem)],
            hasNext ? cursorOf(kept[^1]).Encode() : null);
    }
}
