using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using RecipEBook.Api.Common;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

using static RecipEBook.Api.Features.Tags.ListTags;

namespace RecipEBook.Api.Features.Recipes;

/// <summary>
/// Query — GET /api/recipes?search=&amp;tag=&amp;limit=&amp;cursor=: a page of recipe summaries,
/// most recently updated first.
/// </summary>
public static class ListRecipes
{
    public record RecipeSummary(Guid Id, string Title, int? TotalTimeMinutes, List<TagSummary> Tags);

    public static async Task<Results<Ok<Page<RecipeSummary>>, ValidationProblem>> Handle(
        string? search,
        string? tag,
        int? limit,
        string? cursor,
        AppDbContext db,
        IStringLocalizer<PageRequest> localizer,
        CancellationToken cancellationToken)
    {
        (PageRequest page, Dictionary<string, string[]> errors) = PageRequest.From(limit, cursor, localizer);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

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

        if (page.After is { } after)
        {
            query = query.Where(r =>
                r.UpdatedAt < after.UpdatedAt
                || (r.UpdatedAt == after.UpdatedAt && r.Id.CompareTo(after.Id) < 0));
        }

        var rows = await query
            .OrderByDescending(r => r.UpdatedAt)
            .ThenByDescending(r => r.Id)
            .Take(page.Limit + 1)
            .Select(r => new
            {
                r.Id,
                r.UpdatedAt,
                r.Title,
                TotalTimeMinutes = r.PrepTimeMinutes == null && r.CookTimeMinutes == null
                    ? (int?)null
                    : (r.PrepTimeMinutes ?? 0) + (r.CookTimeMinutes ?? 0),
                Tags = r.Tags.Select(t => new { t.Key, t.Category }).ToList(),
            })
            .ToListAsync(cancellationToken);

        // Tags are sorted (catalog order, grouped by category) and named in memory: SQL can do
        // neither, the category is stored as text.
        Page<RecipeSummary> recipes = page.ToPage(
            rows,
            r => new PageCursor(r.UpdatedAt, r.Id),
            r => new RecipeSummary(
                r.Id,
                r.Title,
                r.TotalTimeMinutes,
                [
                    .. r.Tags
                        .OrderBy(t => TagCatalog.Position(t.Key))
                        .Select(t => TagSummary.From(t.Key, t.Category)),
                ]));

        return TypedResults.Ok(recipes);
    }
}
