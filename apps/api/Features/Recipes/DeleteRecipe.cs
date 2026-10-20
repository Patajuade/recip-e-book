using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Command — DELETE /api/recipes/{id}: deletes a recipe, returns 204 (404 if it doesn't exist).
/// Its tag links (RecipeTag) are deleted by the database cascade.
/// </summary>
public static class DeleteRecipe
{
    public static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        int deleted = await db.Recipes
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
