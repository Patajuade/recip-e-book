using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;

using RecipEBook.Api.Common;
using RecipEBook.Api.Features.ShoppingLists;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

public class ListShoppingListsPagingTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Pages_ReturnEveryListOnceInOrder_EvenWithTheSameUpdatedAt()
    {
        DateTime now = DateTime.UtcNow;
        List<Guid> ids =
        [
            await TestShoppingLists.AddAsync(factory, "A", now),
            // Three lists updated at the same instant: only the Id can order them.
            await TestShoppingLists.AddAsync(factory, "B", now.AddHours(-1)),
            await TestShoppingLists.AddAsync(factory, "C", now.AddHours(-1)),
            await TestShoppingLists.AddAsync(factory, "D", now.AddHours(-1)),
            await TestShoppingLists.AddAsync(factory, "E", now.AddHours(-2)),
        ];

        List<List<Guid>> pages = [];
        string? cursor = null;
        do
        {
            string url = "/api/shopping-lists?limit=2" + (cursor is null ? "" : $"&cursor={Uri.EscapeDataString(cursor)}");
            Page<ListShoppingLists.ShoppingListSummary> page =
                (await _client.GetFromJsonAsync<Page<ListShoppingLists.ShoppingListSummary>>(url))!;
            pages.Add([.. page.Items.Select(l => l.Id)]);
            cursor = page.NextCursor;
        }
        while (cursor is not null && pages.Count < 10);

        Assert.Equal([2, 2, 1], pages.Select(p => p.Count));
        List<Guid> walked = [.. pages.SelectMany(p => p)];
        Assert.Equal(ids.Order(), walked.Order());
        Assert.Equal(ids[0], walked[0]);
        Assert.Equal(ids[4], walked[^1]);
    }

    [Fact]
    public async Task Limit_OutOfRange_ReturnsValidationProblem()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/shopping-lists?limit={PageRequest.MaxLimit + 1}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("limit", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Cursor_Malformed_ReturnsValidationProblem()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/shopping-lists?cursor=nope");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("cursor", problem!.Errors.Keys);
    }
}
