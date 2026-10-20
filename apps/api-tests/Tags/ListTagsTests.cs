using System.Net.Http.Json;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Features.Tags;

namespace RecipEBook.Api.IntegrationTests.Tags;

public class ListTagsTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_ReturnsTheWholeCatalogInCatalogOrder()
    {
        List<ListTags.TagSummary>? tags = await _client.GetFromJsonAsync<List<ListTags.TagSummary>>("/api/tags");

        Assert.Equal(TagCatalog.Tags.Select(t => t.Key), tags!.Select(t => t.Key));
    }

    [Fact]
    public async Task List_CategoriesAreCamelCase()
    {
        List<ListTags.TagSummary>? tags = await _client.GetFromJsonAsync<List<ListTags.TagSummary>>("/api/tags");

        Assert.Equal(
            ["course", "diet", "mainIngredient", "cookingMethod", "season", "occasion", "cuisine", "practical"],
            tags!.Select(t => t.Category).Distinct());
    }
}
