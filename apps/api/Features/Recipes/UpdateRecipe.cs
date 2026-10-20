using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Command — PUT /api/recipes/{id}: replaces a whole recipe (ingredients, steps and tags
/// included), returns 204. The id comes from the route only.
/// </summary>
public static class UpdateRecipe
{
    public static async Task<Results<NoContent, NotFound, ValidationProblem>> Handle(
        Guid id,
        RecipeInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        Recipe? recipe = await db.Recipes
            .Include(r => r.Tags)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (recipe is null)
        {
            return TypedResults.NotFound();
        }

        (List<Tag> tags, Dictionary<string, string[]> errors) = await input.LoadTagsAsync(db, cancellationToken);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        input.ApplyTo(recipe, tags);
        recipe.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
