using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class AddRecipeToShoppingListTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_Recipe_CopiesItsIngredientsAtTheEndWithTheirSource()
    {
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Carrot soup"));
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        await _client.PostAsJsonAsync($"/api/shopping-lists/{listId}/items", new ShoppingListItemInput("bread"));

        HttpResponseMessage response =
            await _client.PostAsJsonAsync($"/api/shopping-lists/{listId}/recipes", new AddRecipeInput(recipeId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, (await response.Content.ReadFromJsonAsync<AddRecipeToShoppingList.Response>())!.ItemsAdded);
        GetShoppingList.ShoppingListDetails list = await GetAsync(listId);
        Assert.Equal(
            [("bread", null, null, null), ("carrots", 3m, "piece", "Carrot soup"), ("salt", null, null, "Carrot soup")],
            list.Items.Select(i => (i.Name, i.Quantity, i.Unit, i.SourceRecipe?.Title)));
        Assert.Equal([0, 1, 2], list.Items.Select(i => i.Position));
    }

    [Fact]
    public async Task Post_RecipeForOtherServings_ScalesTheQuantities()
    {
        // TestRecipes.Valid: 4 servings, 3 carrots, salt without quantity.
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());
        Guid listId = await TestShoppingLists.CreateAsync(_client);

        await _client.PostAsJsonAsync($"/api/shopping-lists/{listId}/recipes", new AddRecipeInput(recipeId, Servings: 2));

        Assert.Equal([1.5m, null], (await GetAsync(listId)).Items.Select(i => i.Quantity));
    }

    [Fact]
    public async Task Put_RecipeAfterwards_LeavesTheCopiesUnchanged()
    {
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        await _client.PostAsJsonAsync($"/api/shopping-lists/{listId}/recipes", new AddRecipeInput(recipeId));

        await _client.PutAsJsonAsync(
            $"/api/recipes/{recipeId}", TestRecipes.Valid() with { Ingredients = [new IngredientInput("leeks", 2)] });

        Assert.Equal(["carrots", "salt"], (await GetAsync(listId)).Items.Select(i => i.Name));
    }

    [Fact]
    public async Task Post_UnknownRecipe_Returns400InTheRequestLanguage()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        using HttpRequestMessage request = new(HttpMethod.Post, $"/api/shopping-lists/{listId}/recipes")
        {
            Content = JsonContent.Create(new AddRecipeInput(Guid.NewGuid())),
        };
        request.Headers.AcceptLanguage.ParseAdd("fr");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Recette inconnue."], problem!.Errors["RecipeId"]);
        Assert.Empty((await GetAsync(listId)).Items);
    }

    [Fact]
    public async Task Post_UnknownList_Returns404()
    {
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/shopping-lists/{Guid.NewGuid()}/recipes", new AddRecipeInput(recipeId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<GetShoppingList.ShoppingListDetails> GetAsync(Guid listId) =>
        (await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{listId}"))!;
}
