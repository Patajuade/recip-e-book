using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Features.Recipes;

namespace RecipEBook.Api.IntegrationTests.Recipes;

public class UpdateRecipeTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Put_ReplacesTheWholeRecipeAndKeepsCreatedAt()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Before", "soup"));
        GetRecipe.RecipeDetails? before = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");

        RecipeInput update = new(
            Title: "After",
            Servings: 2,
            Ingredients: [new IngredientInput("eggs", 3)],
            Steps: ["Beat the eggs."],
            Tags: ["eggs", "quick"]);
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/recipes/{id}", update);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        GetRecipe.RecipeDetails? after = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.NotNull(after);
        Assert.Equal("After", after.Title);
        Assert.Equal(2, after.Servings);
        Assert.Null(after.Description);
        Assert.Null(after.PrepTimeMinutes);
        Assert.Equal([new GetRecipe.IngredientDetails("eggs", 3, null)], after.Ingredients);
        Assert.Equal(["Beat the eggs."], after.Steps);
        Assert.Equal(["eggs", "quick"], after.Tags);
        Assert.Equal(before!.CreatedAt, after.CreatedAt);
        Assert.True(after.UpdatedAt > before.UpdatedAt);
    }

    [Fact]
    public async Task Put_IgnoresIdAndDatesSentInTheBody()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());
        GetRecipe.RecipeDetails? before = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/recipes/{id}", new
        {
            id = Guid.NewGuid(),
            createdAt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            title = "Mass assignment attempt",
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        GetRecipe.RecipeDetails? after = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.Equal(id, after!.Id);
        Assert.Equal(before!.CreatedAt, after.CreatedAt);
    }

    [Fact]
    public async Task Put_UnknownRecipe_Returns404()
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/recipes/{Guid.NewGuid()}", TestRecipes.Valid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_InvalidBody_Returns400AndChangesNothing()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Unchanged"));

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/recipes/{id}", TestRecipes.Valid() with { Title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        GetRecipe.RecipeDetails? recipe = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.Equal("Unchanged", recipe!.Title);
    }

    [Fact]
    public async Task Put_TagOutsideTheCatalog_Returns400AndKeepsTheTags()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Keep tags", "soup"));

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/recipes/{id}", TestRecipes.Valid("Keep tags", "nope"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Tags[0]"], problem!.Errors.Keys);
        GetRecipe.RecipeDetails? recipe = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.Equal(["soup"], recipe!.Tags);
    }
}
