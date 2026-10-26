using System.Net;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class DeleteShoppingListTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Delete_ExistingList_Returns204ThenTheListIsGone()
    {
        Guid id = await TestShoppingLists.CreateAsync(_client);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/shopping-lists/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/shopping-lists/{id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_Twice_Returns404TheSecondTime()
    {
        Guid id = await TestShoppingLists.CreateAsync(_client);
        await _client.DeleteAsync($"/api/shopping-lists/{id}");

        HttpResponseMessage response = await _client.DeleteAsync($"/api/shopping-lists/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
