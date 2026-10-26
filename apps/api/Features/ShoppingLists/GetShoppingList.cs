using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Query — GET /api/shopping-lists/{id}: one list and its items in list order, or 404.
/// </summary>
public static class GetShoppingList
{
    /// <summary>The recipe an item was copied from, as it is now (null once deleted).</summary>
    public record SourceRecipe(Guid Id, string Title);

    public record ItemDetails(
        Guid Id,
        string Name,
        decimal? Quantity,
        string? Unit,
        bool Checked,
        int Position,
        SourceRecipe? SourceRecipe);

    public record ShoppingListDetails(
        Guid Id,
        string Name,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        List<ItemDetails> Items);

    public static async Task<Results<Ok<ShoppingListDetails>, NotFound>> Handle(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        ShoppingListDetails? list = await db.ShoppingLists
            .Where(l => l.Id == id)
            .Select(l => new ShoppingListDetails(
                l.Id,
                l.Name,
                l.CreatedAt,
                l.UpdatedAt,
                l.Items
                    .OrderBy(i => i.Position)
                    .Select(i => new ItemDetails(
                        i.Id,
                        i.Name,
                        i.Quantity,
                        i.Unit,
                        i.Checked,
                        i.Position,
                        i.SourceRecipe == null ? null : new SourceRecipe(i.SourceRecipe.Id, i.SourceRecipe.Title)))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return list is null ? TypedResults.NotFound() : TypedResults.Ok(list);
    }
}
