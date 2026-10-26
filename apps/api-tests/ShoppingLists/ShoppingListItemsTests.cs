using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class ShoppingListItemsTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_Item_Returns201AndAddsItAtTheEnd()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        await AddAsync(listId, new ShoppingListItemInput("bread"));

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/shopping-lists/{listId}/items", new ShoppingListItemInput("  milk  ", 1, "  "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        GetShoppingList.ShoppingListDetails list = await GetAsync(listId);
        Assert.Equal(["bread", "milk"], list.Items.Select(i => i.Name));
        Assert.Equal((1m, (string?)null, 1), (list.Items[1].Quantity, list.Items[1].Unit, list.Items[1].Position));
    }

    [Fact]
    public async Task Post_InvalidItem_Returns400ByField()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/shopping-lists/{listId}/items", new ShoppingListItemInput(" ", -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Name", "Quantity"], problem!.Errors.Keys.Order());
    }

    [Fact]
    public async Task Post_UnknownList_Returns404()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/shopping-lists/{Guid.NewGuid()}/items", new ShoppingListItemInput("bread"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_CheckedItem_Returns204AndUpdatesTheListDate()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        Guid itemId = await AddAsync(listId, new ShoppingListItemInput("bread"));
        DateTime before = (await GetAsync(listId)).UpdatedAt;

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/shopping-lists/{listId}/items/{itemId}", new ShoppingListItemInput("bread", Checked: true));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        GetShoppingList.ShoppingListDetails list = await GetAsync(listId);
        Assert.True(list.Items.Single().Checked);
        Assert.True(list.UpdatedAt > before);
    }

    [Fact]
    public async Task Put_ItemThroughAnotherListsUrl_Returns404AndChangesNothing()
    {
        Guid listA = await TestShoppingLists.CreateAsync(_client, "A");
        Guid listB = await TestShoppingLists.CreateAsync(_client, "B");
        Guid itemOfA = await AddAsync(listA, new ShoppingListItemInput("bread"));

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/shopping-lists/{listB}/items/{itemOfA}", new ShoppingListItemInput("hacked", Checked: true));
        HttpResponseMessage delete = await _client.DeleteAsync($"/api/shopping-lists/{listB}/items/{itemOfA}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        GetShoppingList.ItemDetails item = (await GetAsync(listA)).Items.Single();
        Assert.Equal(("bread", false), (item.Name, item.Checked));
    }

    [Fact]
    public async Task Delete_Item_Returns204ThenItIsGone()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        Guid itemId = await AddAsync(listId, new ShoppingListItemInput("bread"));

        HttpResponseMessage first = await _client.DeleteAsync($"/api/shopping-lists/{listId}/items/{itemId}");
        HttpResponseMessage second = await _client.DeleteAsync($"/api/shopping-lists/{listId}/items/{itemId}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        Assert.Empty((await GetAsync(listId)).Items);
    }

    [Fact]
    public async Task DeleteCheckedItems_KeepsTheOthers()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_client);
        await AddAsync(listId, new ShoppingListItemInput("bread", Checked: true));
        await AddAsync(listId, new ShoppingListItemInput("milk"));
        await AddAsync(listId, new ShoppingListItemInput("eggs", Checked: true));

        HttpResponseMessage response = await _client.DeleteAsync($"/api/shopping-lists/{listId}/checked-items");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(["milk"], (await GetAsync(listId)).Items.Select(i => i.Name));
    }

    private async Task<Guid> AddAsync(Guid listId, ShoppingListItemInput item)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/shopping-lists/{listId}/items", item);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AddShoppingListItem.Response>())!.Id;
    }

    private async Task<GetShoppingList.ShoppingListDetails> GetAsync(Guid listId) =>
        (await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{listId}"))!;
}
