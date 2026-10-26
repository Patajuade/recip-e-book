using System.Net;
using System.Net.Http.Json;

using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.Tags;

namespace RecipEBook.Api.IntegrationTests.Recipes;

public class GetRecipeTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_ExistingRecipe_ReturnsAllItsFields()
    {
        DateTime before = DateTime.UtcNow;
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Carrot soup", "eggs", "soup", "vegetarian"));

        GetRecipe.RecipeDetails? recipe = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");

        Assert.NotNull(recipe);
        Assert.Equal(id, recipe.Id);
        Assert.Equal("Carrot soup", recipe.Title);
        Assert.Equal(4, recipe.Servings);
        Assert.Equal(
            [new GetRecipe.IngredientDetails("carrots", 3, "piece"), new GetRecipe.IngredientDetails("salt", null, null)],
            recipe.Ingredients);
        Assert.Equal(["Cut the carrots.", "Cook them."], recipe.Steps);
        Assert.Equal(
            [
                new ListTags.TagSummary("soup", "course"),
                new ListTags.TagSummary("vegetarian", "diet"),
                new ListTags.TagSummary("eggs", "mainIngredient"),
            ],
            recipe.Tags);
        Assert.Equal(DateTimeKind.Utc, recipe.CreatedAt.Kind);
        Assert.InRange(recipe.CreatedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task Get_DatesAreSerializedAsUtc()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());

        string json = await _client.GetStringAsync($"/api/recipes/{id}");

        Assert.Matches("\"createdAt\":\"[^\"]+Z\"", json);
    }

    [Fact]
    public async Task Get_UnknownRecipe_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_IdThatIsNotAGuid_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/recipes/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
