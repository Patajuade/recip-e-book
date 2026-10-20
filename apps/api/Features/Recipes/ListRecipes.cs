using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Query — GET /api/recipes?search=&amp;tag=: recipe summaries, most recently updated first.
/// </summary>
public static class ListRecipes
{
    public record RecipeSummary(Guid Id, string Title, int? TotalTimeMinutes, List<string> Tags);

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

        List<RecipeSummary> recipes = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new RecipeSummary(
                r.Id,
                r.Title,
                r.PrepTimeMinutes == null && r.CookTimeMinutes == null
                    ? (int?)null
                    : (r.PrepTimeMinutes ?? 0) + (r.CookTimeMinutes ?? 0),
                r.Tags.OrderBy(t => t.Key).Select(t => t.Key).ToList()))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(recipes);
    }
}
