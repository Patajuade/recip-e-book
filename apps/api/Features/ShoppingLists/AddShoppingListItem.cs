using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — POST /api/shopping-lists/{id}/items: adds a free item at the end of the list,
/// returns 201 with its id (404 if the list doesn't exist).
/// </summary>
public static class AddShoppingListItem
{
    public record Response(Guid Id);

    public static async Task<Results<Created<Response>, NotFound>> Handle(
        Guid id,
        ShoppingListItemInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        ShoppingList? list = await db.ShoppingLists.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (list is null)
        {
            return TypedResults.NotFound();
        }

        ShoppingListItem item = new() { Name = "", Position = await NextPositionAsync(db, id, cancellationToken) };
        input.ApplyTo(item);
        list.Items.Add(item);
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        // An item has no endpoint of its own: no Location header.
        return TypedResults.Created((string?)null, new Response(item.Id));
    }

    internal static async Task<int> NextPositionAsync(AppDbContext db, Guid listId, CancellationToken cancellationToken)
    {
        int? last = await db.Set<ShoppingListItem>()
            .Where(i => i.ShoppingListId == listId)
            .MaxAsync(i => (int?)i.Position, cancellationToken);
        return (last ?? -1) + 1;
    }
}
