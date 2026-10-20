using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Write model shared by the commands that receive a whole recipe (POST and PUT).
/// It never contains the id or the dates: the server sets them.
/// </summary>
public record RecipeInput(
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
    // Tags are checked against the catalog by LoadTagsAsync, which needs the database.
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

    /// <summary>
    /// Loads the catalog tags matching <see cref="Tags"/>. Unknown keys are returned as
    /// validation errors, so a client can't silently lose a tag.
    /// </summary>
    internal async Task<(List<Tag> Tags, Dictionary<string, string[]> Errors)> LoadTagsAsync(
        AppDbContext db, CancellationToken cancellationToken)
    {
        List<string> keys = Tags ?? [];
        List<Tag> tags = await db.Tags.Where(t => keys.Contains(t.Key)).ToListAsync(cancellationToken);
        Dictionary<string, string[]> errors = keys
            .Select((key, index) => (key, index))
            .Where(tag => tags.All(t => t.Key != tag.key))
            .ToDictionary(tag => $"Tags[{tag.index}]", tag => new[] { $"Unknown tag '{tag.key}'." });
        return (tags, errors);
    }

    /// <summary>Copies the input into the entity, replacing ingredients, steps and tags.</summary>
    internal void ApplyTo(Recipe recipe, List<Tag> tags)
    {
        recipe.Title = Title.Trim();
        recipe.Description = TrimToNull(Description);
        recipe.Servings = Servings;
        recipe.PrepTimeMinutes = PrepTimeMinutes;
        recipe.CookTimeMinutes = CookTimeMinutes;
        recipe.Notes = TrimToNull(Notes);
        recipe.Ingredients =
        [
            .. (Ingredients ?? []).Select(i => new Ingredient
            {
                Name = i.Name.Trim(),
                Quantity = i.Quantity,
                Unit = TrimToNull(i.Unit),
            }),
        ];
        recipe.Steps = [.. (Steps ?? []).Select(s => new RecipeStep { Instruction = s.Trim() })];
        recipe.Tags.Clear();
        recipe.Tags.AddRange(tags);
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public record IngredientInput(
    [Required, StringLength(200)] string Name,
    [Range(0d, 100_000d)] decimal? Quantity = null,
    [StringLength(50)] string? Unit = null);
