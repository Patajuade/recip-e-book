using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — PUT /api/shopping-lists/{id}: renames a list, returns 204 (404 if it doesn't exist).
/// </summary>
public static class RenameShoppingList
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        ShoppingListInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        ShoppingList? list = await db.ShoppingLists.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (list is null)
        {
            return TypedResults.NotFound();
        }

        list.Name = input.Name.Trim();
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
