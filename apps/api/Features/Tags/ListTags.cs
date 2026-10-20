using System.Text.Json;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Tags;

/// <summary>
/// Query — GET /api/tags: the tag catalog, in catalog order (grouped by category).
/// </summary>
public static class ListTags
{
    /// <param name="Category">camelCase, like the frontend translation keys (`mainIngredient`).</param>
    public record TagSummary(string Key, string Category);

    private static readonly Dictionary<string, int> CatalogOrder = TagCatalog.Tags
        .Select((tag, index) => (tag.Key, index))
        .ToDictionary(tag => tag.Key, tag => tag.index);

    public static async Task<Ok<List<TagSummary>>> Handle(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        List<Tag> tags = await db.Tags.AsNoTracking().ToListAsync(cancellationToken);

        List<TagSummary> catalog = tags
            .OrderBy(t => CatalogOrder.GetValueOrDefault(t.Key, int.MaxValue))
            .Select(t => new TagSummary(t.Key, JsonNamingPolicy.CamelCase.ConvertName(t.Category.ToString())))
            .ToList();

        return TypedResults.Ok(catalog);
    }
}
