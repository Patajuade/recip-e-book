using Microsoft.AspNetCore.Http.HttpResults;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Command — POST /api/recipes: creates a recipe, returns 201 with its location.
/// </summary>
public static class CreateRecipe
{
    public record Response(Guid Id);

    public static async Task<Results<CreatedAtRoute<Response>, ValidationProblem>> Handle(
        RecipeInput input,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        (List<Tag> tags, Dictionary<string, string[]> errors) = await input.LoadTagsAsync(db, cancellationToken);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        DateTime now = DateTime.UtcNow;
        Recipe recipe = new() { Title = "", CreatedAt = now, UpdatedAt = now };
        input.ApplyTo(recipe, tags);

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(new Response(recipe.Id), "GetRecipe", new { id = recipe.Id });
    }
}
