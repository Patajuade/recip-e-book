using Microsoft.AspNetCore.Http.HttpResults;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Command — POST /api/shopping-lists: creates an empty list, returns 201 with its location.
/// </summary>
public static class CreateShoppingList
{
    public record Response(Guid Id);

    public static async Task<CreatedAtRoute<Response>> Handle(
        ShoppingListInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        ShoppingList list = new() { Name = input.Name.Trim(), CreatedAt = now, UpdatedAt = now };

        db.ShoppingLists.Add(list);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(new Response(list.Id), "GetShoppingList", new { id = list.Id });
    }
}
