using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — PUT /api/shopping-lists/{id}/items/{itemId}: replaces an item (name, quantity,
/// unit, checked), returns 204. 404 if the list doesn't exist or the item isn't in it.
/// </summary>
public static class UpdateShoppingListItem
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        Guid itemId,
        ShoppingListItemInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        (ShoppingList? list, ShoppingListItem? item) = await FindAsync(db, id, itemId, cancellationToken);
        if (list is null || item is null)
        {
            return TypedResults.NotFound();
        }

        input.ApplyTo(item);
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    /// <summary>
    /// Loads the list with only that item, and only if the item is in that list: an item id
    /// used with another list's URL is not found (the URL can't reach another list's item).
    /// </summary>
    internal static async Task<(ShoppingList? List, ShoppingListItem? Item)> FindAsync(
        AppDbContext db, Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        ShoppingList? list = await db.ShoppingLists
            .Include(l => l.Items.Where(i => i.Id == itemId))
            .FirstOrDefaultAsync(l => l.Id == listId, cancellationToken);
        return (list, list?.Items.SingleOrDefault());
    }
}
