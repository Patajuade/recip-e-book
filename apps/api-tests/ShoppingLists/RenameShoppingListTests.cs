using System.Net;
using System.Net.Http.Json;

using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class RenameShoppingListTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Put_NewName_Returns204AndRenamesTheList()
    {
        Guid id = await TestShoppingLists.CreateAsync(_client, "Week");
        GetShoppingList.ShoppingListDetails? before =
            await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{id}");

        HttpResponseMessage response =
            await _client.PutAsJsonAsync($"/api/shopping-lists/{id}", new ShoppingListInput("Birthday"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        GetShoppingList.ShoppingListDetails? after =
            await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{id}");
        Assert.Equal("Birthday", after!.Name);
        Assert.True(after.UpdatedAt > before!.UpdatedAt);
    }

    [Fact]
    public async Task Put_UnknownList_Returns404()
    {
        HttpResponseMessage response =
            await _client.PutAsJsonAsync($"/api/shopping-lists/{Guid.NewGuid()}", new ShoppingListInput("Birthday"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
