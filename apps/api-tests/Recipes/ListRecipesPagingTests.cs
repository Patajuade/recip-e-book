using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Common;
using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests.Recipes;

// Each test searches its own title prefix, so the recipes of the other tests don't count.
public class ListRecipesPagingTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Pages_ReturnEveryRecipeOnceInOrder_EvenWithTheSameUpdatedAt()
    {
        List<Guid> ids = await CreateAsync("Walk", 5);
        // Three recipes updated at the same instant: only the Id can order them.
        await SetUpdatedAtAsync(ids[1..4], DateTime.UtcNow.AddDays(-1));

        List<List<Guid>> pages = await WalkAsync("/api/recipes?search=walk&limit=2");
        List<Guid> all = (await GetAsync("/api/recipes?search=walk&limit=50")).Items.Select(r => r.Id).ToList();

        Assert.Equal([2, 2, 1], pages.Select(p => p.Count));
        Assert.Equal(all, pages.SelectMany(p => p));
        Assert.Equal(ids.Order(), all.Order());
    }

    [Fact]
    public async Task Pages_KeepTheFilters()
    {
        await CreateAsync("Filtered", 3, "italian");
        await CreateAsync("Filtered", 2, "french");

        List<List<Guid>> pages = await WalkAsync("/api/recipes?search=filtered&tag=italian&limit=2");

        Assert.Equal([2, 1], pages.Select(p => p.Count));
        Page<ListRecipes.RecipeSummary> page = await GetAsync("/api/recipes?search=filtered&tag=italian&limit=50");
        Assert.All(page.Items, r => Assert.Contains(r.Tags, t => t.Key == "italian"));
    }

    [Fact]
    public async Task Limit_DefaultsTo20()
    {
        await CreateAsync("Default", PageRequest.DefaultLimit + 1);

        Page<ListRecipes.RecipeSummary> first = await GetAsync("/api/recipes?search=default");

        Assert.Equal(PageRequest.DefaultLimit, first.Items.Count);
        Assert.NotNull(first.NextCursor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(PageRequest.MaxLimit + 1)]
    public async Task Limit_OutOfRange_ReturnsValidationProblem(int limit)
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes?limit={limit}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["The limit must be between 1 and 50."], problem!.Errors["limit"]);
    }

    [Theory]
    [InlineData("not-a-cursor")]
    [InlineData("MTIzOm5vdC1hLWd1aWQ")] // "123:not-a-guid"
    [InlineData("OTAwMDAwMDAwMDAwMDAwMDAwMDowMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDA")] // past DateTime.MaxValue
    public async Task Cursor_Malformed_ReturnsValidationProblem(string cursor)
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes?cursor={cursor}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["The cursor is not valid."], problem!.Errors["cursor"]);
    }

    [Fact]
    public async Task Cursor_Malformed_InFrench()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/recipes?cursor=nope");
        request.Headers.AcceptLanguage.ParseAdd("fr");

        HttpResponseMessage response = await _client.SendAsync(request);

        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Le curseur n'est pas valide."], problem!.Errors["cursor"]);
    }

    private async Task<List<Guid>> CreateAsync(string prefix, int count, params string[] tags)
    {
        List<Guid> ids = [];
        for (int i = 0; i < count; i++)
        {
            ids.Add(await TestRecipes.CreateAsync(_client, TestRecipes.Valid($"{prefix} {i}", tags)));
        }
        return ids;
    }

    private async Task SetUpdatedAtAsync(List<Guid> ids, DateTime updatedAt)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Recipes
            .Where(r => ids.Contains(r.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UpdatedAt, updatedAt));
    }

    private async Task<Page<ListRecipes.RecipeSummary>> GetAsync(string url) =>
        (await _client.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>(url))!;

    // Follows NextCursor to the last page, as a client would.
    private async Task<List<List<Guid>>> WalkAsync(string url)
    {
        List<List<Guid>> pages = [];
        string? cursor = null;
        do
        {
            Page<ListRecipes.RecipeSummary> page =
                await GetAsync(cursor is null ? url : $"{url}&cursor={Uri.EscapeDataString(cursor)}");
            pages.Add([.. page.Items.Select(r => r.Id)]);
            cursor = page.NextCursor;
        }
        while (cursor is not null && pages.Count < 10);
        return pages;
    }
}
