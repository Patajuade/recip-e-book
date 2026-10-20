using System.Net;
using System.Net.Http.Json;

using RecipEBook.Api.Features.Recipes;

namespace RecipEBook.Api.IntegrationTests;

/// <summary>Builds valid recipe bodies and creates recipes through the API.</summary>
internal static class TestRecipes
{
    public static RecipeInput Valid(string title = "Test soup", params string[] tags) => new(
        Title: title,
        Description: "A test recipe.",
        Servings: 4,
        PrepTimeMinutes: 10,
        CookTimeMinutes: 20,
        Notes: null,
        Ingredients: [new IngredientInput("carrots", 3, "piece"), new IngredientInput("salt")],
        Steps: ["Cut the carrots.", "Cook them."],
        Tags: tags.Length > 0 ? [.. tags] : ["soup", "vegetarian"]);

    public static async Task<Guid> CreateAsync(HttpClient client, RecipeInput recipe)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/recipes", recipe);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CreateRecipe.Response? body = await response.Content.ReadFromJsonAsync<CreateRecipe.Response>();
        return body!.Id;
    }
}
