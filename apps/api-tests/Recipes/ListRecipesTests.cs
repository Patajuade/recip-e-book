using System.Net.Http.Json;

using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.Tags;

namespace RecipEBook.Api.IntegrationTests.Recipes;

public class ListRecipesTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_SearchIgnoresCaseAndAccents()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Soupe de légumes d'hiver"));

        List<ListRecipes.RecipeSummary>? recipes =
            await _client.GetFromJsonAsync<List<ListRecipes.RecipeSummary>>("/api/recipes?search=LEGUMES");

        Assert.Contains(recipes!, r => r.Id == id);
    }

    [Fact]
    public async Task List_TagFilterReturnsOnlyTaggedRecipes()
    {
        Guid tagged = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Tagged", "italian"));
        Guid other = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Not tagged", "french"));

        List<ListRecipes.RecipeSummary>? recipes =
            await _client.GetFromJsonAsync<List<ListRecipes.RecipeSummary>>("/api/recipes?tag=italian");

        Assert.Contains(recipes!, r => r.Id == tagged);
        Assert.DoesNotContain(recipes!, r => r.Id == other);
        Assert.All(recipes!, r => Assert.Contains(r.Tags, t => t.Key == "italian"));
    }

    [Fact]
    public async Task List_ReturnsTotalTimeAndTagsGroupedByCategory()
    {
        Guid id = await TestRecipes.CreateAsync(
            _client, TestRecipes.Valid("Summary", "italian", "vegetarian", "eggs", "soup", "quick"));

        List<ListRecipes.RecipeSummary>? recipes =
            await _client.GetFromJsonAsync<List<ListRecipes.RecipeSummary>>("/api/recipes?search=summary");

        ListRecipes.RecipeSummary summary = Assert.Single(recipes!, r => r.Id == id);
        Assert.Equal(30, summary.TotalTimeMinutes);
        Assert.Equal(
            [
                // Catalog order: dish type, diet, main ingredient, …, cuisine, practical.
                new ListTags.TagSummary("soup", "course"),
                new ListTags.TagSummary("vegetarian", "diet"),
                new ListTags.TagSummary("eggs", "mainIngredient"),
                new ListTags.TagSummary("italian", "cuisine"),
                new ListTags.TagSummary("quick", "practical"),
            ],
            summary.Tags);
    }

    [Fact]
    public async Task List_MostRecentlyUpdatedFirst()
    {
        Guid first = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Order A"));
        Guid second = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Order B"));

        List<ListRecipes.RecipeSummary>? recipes =
            await _client.GetFromJsonAsync<List<ListRecipes.RecipeSummary>>("/api/recipes?search=order");

        Assert.Equal([second, first], recipes!.Select(r => r.Id));
    }
}
