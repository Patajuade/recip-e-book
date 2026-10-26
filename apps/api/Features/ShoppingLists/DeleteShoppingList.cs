using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — DELETE /api/shopping-lists/{id}: deletes a list, returns 204 (404 if it doesn't exist).
/// Its items are deleted by the database cascade.
/// </summary>
public static class DeleteShoppingList
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        int deleted = await db.ShoppingLists
            .Where(l => l.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }
}
