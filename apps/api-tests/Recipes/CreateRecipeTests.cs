using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Features.Recipes;

namespace RecipEBook.Api.IntegrationTests.Recipes;

public class CreateRecipeTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ValidRecipe_Returns201WithLocationOfTheNewRecipe()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recipes", TestRecipes.Valid());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        CreateRecipe.Response? body = await response.Content.ReadFromJsonAsync<CreateRecipe.Response>();
        Assert.NotNull(body);
        Assert.Equal($"/api/recipes/{body.Id}", response.Headers.Location?.AbsolutePath);

        HttpResponseMessage created = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
    }

    [Fact]
    public async Task Post_TrimsTextsAndStoresEmptyOnesAsNull()
    {
        RecipeInput input = TestRecipes.Valid() with
        {
            Title = "  Trimmed title  ",
            Description = "   ",
            Ingredients = [new IngredientInput(" flour ", 200, " g ")],
            Steps = ["  Mix.  "],
        };

        Guid id = await TestRecipes.CreateAsync(_client, input);

        GetRecipe.RecipeDetails? recipe = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.NotNull(recipe);
        Assert.Equal("Trimmed title", recipe.Title);
        Assert.Null(recipe.Description);
        Assert.Equal(new GetRecipe.IngredientDetails("flour", 200, "g"), recipe.Ingredients.Single());
        Assert.Equal(["Mix."], recipe.Steps);
    }

    [Fact]
    public async Task Post_InvalidFields_Returns400WithErrorsByField()
    {
        RecipeInput input = TestRecipes.Valid() with
        {
            Title = " ",
            Servings = 0,
            Ingredients = [new IngredientInput("", -1)],
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recipes", input);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(
            ["Ingredients[0].Name", "Ingredients[0].Quantity", "Servings", "Title"],
            problem.Errors.Keys.Order());
    }

    [Fact]
    public async Task Post_EmptyStep_Returns400ForThatStep()
    {
        RecipeInput input = TestRecipes.Valid() with { Steps = ["Ok.", "  "] };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recipes", input);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Steps[1]"], problem!.Errors.Keys);
    }

    [Fact]
    public async Task Post_TagOutsideTheCatalog_Returns400AndCreatesNothing()
    {
        string title = $"Unknown tag {Guid.NewGuid()}";
        RecipeInput input = TestRecipes.Valid(title, "vegetarian", "not-a-tag");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/recipes", input);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Unknown tag 'not-a-tag'."], problem!.Errors["Tags[1]"]);

        List<ListRecipes.RecipeSummary>? recipes =
            await _client.GetFromJsonAsync<List<ListRecipes.RecipeSummary>>($"/api/recipes?search={Uri.EscapeDataString(title)}");
        Assert.Empty(recipes!);
    }

    [Fact]
    public async Task Post_DuplicateTags_AreStoredOnce()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Duplicates", "quick", "eggs", "quick"));

        GetRecipe.RecipeDetails? recipe = await _client.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.Equal(["eggs", "quick"], recipe!.Tags);
    }
}
