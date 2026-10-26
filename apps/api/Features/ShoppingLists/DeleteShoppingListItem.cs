using Microsoft.AspNetCore.Http.HttpResults;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — DELETE /api/shopping-lists/{id}/items/{itemId}: removes an item, returns 204.
/// 404 if the list doesn't exist or the item isn't in it.
/// </summary>
public static class DeleteShoppingListItem
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        Guid itemId,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        (ShoppingList? list, ShoppingListItem? item) =
            await UpdateShoppingListItem.FindAsync(db, id, itemId, cancellationToken);
        if (list is null || item is null)
        {
            return TypedResults.NotFound();
        }

        list.Items.Remove(item);
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
