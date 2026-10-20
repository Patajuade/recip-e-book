using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Command — POST /api/recipes: creates a recipe, returns 201 with its location.
/// </summary>
public static class CreateRecipe
{
    public record IngredientInput(
        [Required, StringLength(200)] string Name,
        [Range(0d, 100_000d)] decimal? Quantity = null,
        [StringLength(50)] string? Unit = null);

    public record Request(
        [Required, StringLength(200)] string Title,
        [StringLength(2000)] string? Description = null,
        [Range(1, 100)] int? Servings = null,
        [Range(0, 1440)] int? PrepTimeMinutes = null,
        [Range(0, 1440)] int? CookTimeMinutes = null,
        [StringLength(4000)] string? Notes = null,
        List<IngredientInput>? Ingredients = null,
        List<string>? Steps = null,
        List<string>? Tags = null) : IValidatableObject
    {
        // Attributes can't target each string of a list, so steps are checked here.
        // Tags are checked against the catalog in the handler, which needs the database.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            for (int i = 0; i < (Steps?.Count ?? 0); i++)
            {
                string step = Steps![i];
                if (string.IsNullOrWhiteSpace(step))
                {
                    yield return new ValidationResult("The step is required.", [$"Steps[{i}]"]);
                }
                else if (step.Trim().Length > 2000)
                {
                    yield return new ValidationResult(
                        "The step must be a string with a maximum length of 2000.", [$"Steps[{i}]"]);
                }
            }
        }
    }

    public record Response(Guid Id);

    public static async Task<Results<CreatedAtRoute<Response>, ValidationProblem>> Handle(
        Request request,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        List<string> tagKeys = request.Tags ?? [];
        List<Tag> tags = await db.Tags
            .Where(t => tagKeys.Contains(t.Key))
            .ToListAsync(cancellationToken);
        Dictionary<string, string[]> unknownTags = tagKeys
            .Select((key, index) => (key, index))
            .Where(tag => tags.All(t => t.Key != tag.key))
            .ToDictionary(tag => $"Tags[{tag.index}]", tag => new[] { $"Unknown tag '{tag.key}'." });
        if (unknownTags.Count > 0)
        {
            return TypedResults.ValidationProblem(unknownTags);
        }

        DateTime now = DateTime.UtcNow;
        Recipe recipe = new()
        {
            Title = request.Title.Trim(),
            Description = TrimToNull(request.Description),
            Servings = request.Servings,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Notes = TrimToNull(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
            Ingredients =
            [
                .. (request.Ingredients ?? []).Select(i => new Ingredient
                {
                    Name = i.Name.Trim(),
                    Quantity = i.Quantity,
                    Unit = TrimToNull(i.Unit),
                }),
            ],
            Steps = [.. (request.Steps ?? []).Select(s => new RecipeStep { Instruction = s.Trim() })],
        };
        recipe.Tags.AddRange(tags);

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(new Response(recipe.Id), "GetRecipe", new { id = recipe.Id });
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
