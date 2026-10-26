using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

using static RecipEBook.Api.Features.Tags.ListTags;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Query — GET /api/recipes?search=&amp;tag=: recipe summaries, most recently updated first.
/// </summary>
public static class ListRecipes
{
    public record RecipeSummary(Guid Id, string Title, int? TotalTimeMinutes, List<TagSummary> Tags);

    public static async Task<Ok<List<RecipeSummary>>> Handle(
        string? search,
        string? tag,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        IQueryable<Recipe> query = db.Recipes;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalizedSearch = SearchText.Normalize(search.Trim());
            query = query.Where(r => r.SearchTitle.Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(r => r.Tags.Any(t => t.Key == tag));
        }

        var rows = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new
            {
                r.Id,
                r.Title,
                TotalTimeMinutes = r.PrepTimeMinutes == null && r.CookTimeMinutes == null
                    ? (int?)null
                    : (r.PrepTimeMinutes ?? 0) + (r.CookTimeMinutes ?? 0),
                Tags = r.Tags.Select(t => new { t.Key, t.Category }).ToList(),
            })
            .ToListAsync(cancellationToken);

        // Tags are sorted (catalog order, grouped by category) and named in memory: SQL can do
        // neither, the category is stored as text.
        List<RecipeSummary> recipes =
        [
            .. rows.Select(r => new RecipeSummary(
                r.Id,
                r.Title,
                r.TotalTimeMinutes,
                [
                    .. r.Tags
                        .OrderBy(t => TagCatalog.Position(t.Key))
                        .Select(t => TagSummary.From(t.Key, t.Category)),
                ])),
        ];

        return TypedResults.Ok(recipes);
    }
}
