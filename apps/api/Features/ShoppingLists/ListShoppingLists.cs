using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using RecipEBook.Api.Common;
using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Query — GET /api/shopping-lists?limit=&amp;cursor=: a page of lists with how many items they
/// have and how many are checked, most recently updated first.
/// </summary>
public static class ListShoppingLists
{
    public record ShoppingListSummary(Guid Id, string Name, int ItemCount, int CheckedCount, DateTime UpdatedAt);

    public static async Task<Results<Ok<Page<ShoppingListSummary>>, ValidationProblem>> Handle(
        int? limit,
        string? cursor,
        AppDbContext db,
        IStringLocalizer<PageRequest> localizer,
        CancellationToken cancellationToken)
    {
        (PageRequest page, Dictionary<string, string[]> errors) = PageRequest.From(limit, cursor, localizer);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        IQueryable<ShoppingList> query = db.ShoppingLists;

        if (page.After is { } after)
        {
            query = query.Where(l =>
                l.UpdatedAt < after.UpdatedAt
                || (l.UpdatedAt == after.UpdatedAt && l.Id.CompareTo(after.Id) < 0));
        }

        List<ShoppingListSummary> rows = await query
            .OrderByDescending(l => l.UpdatedAt)
            .ThenByDescending(l => l.Id)
            .Take(page.Limit + 1)
            .Select(l => new ShoppingListSummary(
                l.Id,
                l.Name,
                l.Items.Count,
                l.Items.Count(i => i.Checked),
                l.UpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(page.ToPage(rows, l => new PageCursor(l.UpdatedAt, l.Id), l => l));
    }
}
