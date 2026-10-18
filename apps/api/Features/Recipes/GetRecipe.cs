using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Query — GET /api/recipes/{id}: full details of one recipe, or 404.
/// </summary>
public static class GetRecipe
{
    public record IngredientDetails(string Name, decimal? Quantity, string? Unit);

    public record RecipeDetails(
        Guid Id,
        string Title,
        string? Description,
        int? Servings,
        int? PrepTimeMinutes,
        int? CookTimeMinutes,
        string? Notes,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        List<IngredientDetails> Ingredients,
        List<string> Steps,
        List<string> Tags);

    public static async Task<Results<Ok<RecipeDetails>, NotFound>> Handle(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        // The JSON collections (Ingredients, Steps) are read as whole columns and mapped
        // in memory: projecting their elements in SQL needs an APPLY operation, which
        // SQLite doesn't support.
        var row = await db.Recipes
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                r.Servings,
                r.PrepTimeMinutes,
                r.CookTimeMinutes,
                r.Notes,
                r.CreatedAt,
                r.UpdatedAt,
                r.Ingredients,
                r.Steps,
                Tags = r.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new RecipeDetails(
            row.Id,
            row.Title,
            row.Description,
            row.Servings,
            row.PrepTimeMinutes,
            row.CookTimeMinutes,
            row.Notes,
            row.CreatedAt,
            row.UpdatedAt,
            [.. row.Ingredients.Select(i => new IngredientDetails(i.Name, i.Quantity, i.Unit))],
            [.. row.Steps.Select(s => s.Instruction)],
            row.Tags));
    }
}
