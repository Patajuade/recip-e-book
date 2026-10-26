using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class CreateShoppingListTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_ValidName_Returns201AndAnEmptyListWithTheTrimmedName()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shopping-lists", new ShoppingListInput("  Week  "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        GetShoppingList.ShoppingListDetails? list =
            await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>(response.Headers.Location);
        Assert.Equal("Week", list!.Name);
        Assert.Empty(list.Items);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("A name that is way too long for a shopping list, which only needs a few words to be found again later on")]
    public async Task Post_BlankOrTooLongName_Returns400ForTheName(string name)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/shopping-lists", new ShoppingListInput(name));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Name"], problem!.Errors.Keys);
    }
}
