using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — DELETE /api/shopping-lists/{id}/checked-items: removes the checked items once
/// shopping is done, returns 204 (404 if the list doesn't exist).
/// </summary>
public static class ClearCheckedItems
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        ShoppingList? list = await db.ShoppingLists.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (list is null)
        {
            return TypedResults.NotFound();
        }

        await db.Set<ShoppingListItem>()
            .Where(i => i.ShoppingListId == id && i.Checked)
            .ExecuteDeleteAsync(cancellationToken);
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
