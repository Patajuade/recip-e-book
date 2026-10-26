using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Query — GET /api/shopping-lists: the lists with how many items they have and how many
/// are checked, most recently updated first.
/// </summary>
public static class ListShoppingLists
{
    public record ShoppingListSummary(Guid Id, string Name, int ItemCount, int CheckedCount, DateTime UpdatedAt);

    public static async Task<Ok<List<ShoppingListSummary>>> Handle(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        List<ShoppingListSummary> lists = await db.ShoppingLists
            .OrderByDescending(l => l.UpdatedAt)
            .Select(l => new ShoppingListSummary(
                l.Id,
                l.Name,
                l.Items.Count,
                l.Items.Count(i => i.Checked),
                l.UpdatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(lists);
    }
}
