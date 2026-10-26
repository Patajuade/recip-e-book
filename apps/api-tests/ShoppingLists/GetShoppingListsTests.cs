using System.Net;
using System.Net.Http.Json;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class GetShoppingListsTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_ReturnsCountsMostRecentlyUpdatedFirst()
    {
        DateTime now = DateTime.UtcNow;
        Guid older = await TestShoppingLists.AddAsync(factory, "Older", now.AddDays(-1), new ShoppingListItem { Name = "bread" });
        Guid newer = await TestShoppingLists.AddAsync(
            factory, "Newer", now, new ShoppingListItem { Name = "milk", Checked = true }, new ShoppingListItem { Name = "eggs" });

        List<ListShoppingLists.ShoppingListSummary>? lists =
            await _client.GetFromJsonAsync<List<ListShoppingLists.ShoppingListSummary>>("/api/shopping-lists");

        List<Guid> ids = [.. lists!.Select(l => l.Id)];
        Assert.True(ids.IndexOf(newer) < ids.IndexOf(older));
        ListShoppingLists.ShoppingListSummary summary = lists!.Single(l => l.Id == newer);
        Assert.Equal(("Newer", 2, 1), (summary.Name, summary.ItemCount, summary.CheckedCount));
    }

    [Fact]
    public async Task Get_ReturnsItemsInListOrderWithTheirSourceRecipe()
    {
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Carrot soup"));
        Guid id = await TestShoppingLists.AddAsync(
            factory,
            "Week",
            DateTime.UtcNow,
            new ShoppingListItem { Name = "carrots", Quantity = 3, Unit = "piece", SourceRecipeId = recipeId },
            new ShoppingListItem { Name = "bread", Checked = true });

        GetShoppingList.ShoppingListDetails? list =
            await _client.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{id}");

        Assert.Equal("Week", list!.Name);
        Assert.Equal(["carrots", "bread"], list.Items.Select(i => i.Name));
        Assert.Equal(new GetShoppingList.SourceRecipe(recipeId, "Carrot soup"), list.Items[0].SourceRecipe);
        Assert.Equal((3m, "piece", false), (list.Items[0].Quantity, list.Items[0].Unit, list.Items[0].Checked));
        Assert.Null(list.Items[1].SourceRecipe);
        Assert.True(list.Items[1].Checked);
    }

    [Fact]
    public async Task Get_UnknownList_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/shopping-lists/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
