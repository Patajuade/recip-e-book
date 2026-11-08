using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Common;
using RecipEBook.Api.Domain;
using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.ShoppingLists;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests.Accounts;

/// <summary>Two users on the same API: each only reaches their own recipes and lists.</summary>
public partial class OwnershipTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _alice = TestAccounts.SignedIn(factory);
    private readonly HttpClient _bob = TestAccounts.SignedIn(factory);

    [Fact]
    public async Task Recipe_OfAnotherUser_CantBeReadChangedOrDeleted()
    {
        Guid id = await TestRecipes.CreateAsync(_alice, TestRecipes.Valid("Alice's soup"));

        Assert.Equal(HttpStatusCode.NotFound, (await _bob.GetAsync($"/api/recipes/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _bob.PutAsJsonAsync($"/api/recipes/{id}", TestRecipes.Valid("Taken"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _bob.DeleteAsync($"/api/recipes/{id}")).StatusCode);

        GetRecipe.RecipeDetails? recipe = await _alice.GetFromJsonAsync<GetRecipe.RecipeDetails>($"/api/recipes/{id}");
        Assert.Equal("Alice's soup", recipe!.Title);
    }

    [Fact]
    public async Task RecipeList_ShowsOnlyOnesOwnRecipes()
    {
        string title = $"Owned {Guid.NewGuid():N}";
        Guid id = await TestRecipes.CreateAsync(_alice, TestRecipes.Valid(title));

        Page<ListRecipes.RecipeSummary>? alices =
            await _alice.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>($"/api/recipes?search={title}");
        Page<ListRecipes.RecipeSummary>? bobs =
            await _bob.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>($"/api/recipes?search={title}");

        Assert.Equal([id], alices!.Items.Select(r => r.Id));
        Assert.Empty(bobs!.Items);
    }

    [Fact]
    public async Task ShoppingList_OfAnotherUser_CantBeReadOrChanged()
    {
        Guid listId = await TestShoppingLists.CreateAsync(_alice, "Alice's week");
        HttpResponseMessage added = await _alice.PostAsJsonAsync(
            $"/api/shopping-lists/{listId}/items", new ShoppingListItemInput("bread", null, null, false));
        Guid itemId = (await added.Content.ReadFromJsonAsync<AddShoppingListItem.Response>())!.Id;
        Guid recipeId = await TestRecipes.CreateAsync(_bob, TestRecipes.Valid("Bob's soup"));

        HttpResponseMessage[] responses =
        [
            await _bob.GetAsync($"/api/shopping-lists/{listId}"),
            await _bob.PutAsJsonAsync($"/api/shopping-lists/{listId}", new ShoppingListInput("Taken")),
            await _bob.PostAsJsonAsync($"/api/shopping-lists/{listId}/items", new ShoppingListItemInput("milk", null, null, false)),
            await _bob.PutAsJsonAsync($"/api/shopping-lists/{listId}/items/{itemId}", new ShoppingListItemInput("cake", null, null, true)),
            await _bob.DeleteAsync($"/api/shopping-lists/{listId}/items/{itemId}"),
            await _bob.DeleteAsync($"/api/shopping-lists/{listId}/checked-items"),
            await _bob.PostAsJsonAsync($"/api/shopping-lists/{listId}/recipes", new AddRecipeInput(recipeId, null)),
            await _bob.DeleteAsync($"/api/shopping-lists/{listId}"),
        ];

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode));
        GetShoppingList.ShoppingListDetails? list =
            await _alice.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{listId}");
        Assert.Equal("Alice's week", list!.Name);
        Assert.Equal(["bread"], list.Items.Select(i => i.Name));
        Assert.False(list.Items.Single().Checked);
    }

    [Fact]
    public async Task AnotherUsersRecipe_CantBeAddedToOnesOwnList()
    {
        Guid alicesRecipe = await TestRecipes.CreateAsync(_alice, TestRecipes.Valid("Alice's secret"));
        Guid bobsList = await TestShoppingLists.CreateAsync(_bob, "Bob's week");

        HttpResponseMessage response = await _bob.PostAsJsonAsync(
            $"/api/shopping-lists/{bobsList}/recipes", new AddRecipeInput(alicesRecipe, null));
        HttpResponseMessage unknown = await _bob.PostAsJsonAsync(
            $"/api/shopping-lists/{bobsList}/recipes", new AddRecipeInput(Guid.NewGuid(), null));

        // The same answer as for a recipe that doesn't exist: nothing tells it does.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(unknown.StatusCode, response.StatusCode);
        Assert.Equal(
            (await unknown.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Http.HttpValidationProblemDetails>())!.Errors,
            (await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Http.HttpValidationProblemDetails>())!.Errors);
        GetShoppingList.ShoppingListDetails? list =
            await _bob.GetFromJsonAsync<GetShoppingList.ShoppingListDetails>($"/api/shopping-lists/{bobsList}");
        Assert.Empty(list!.Items);
    }

    [Fact]
    public async Task ShoppingListList_ShowsOnlyOnesOwnLists()
    {
        Guid alices = await TestShoppingLists.CreateAsync(_alice, "Alice's");
        Guid bobs = await TestShoppingLists.CreateAsync(_bob, "Bob's");

        Page<ListShoppingLists.ShoppingListSummary>? page =
            await _bob.GetFromJsonAsync<Page<ListShoppingLists.ShoppingListSummary>>("/api/shopping-lists?limit=50");

        Assert.Contains(page!.Items, l => l.Id == bobs);
        Assert.DoesNotContain(page.Items, l => l.Id == alices);
    }

    [Fact]
    public async Task EveryApiEndpoint_ButTheAccountsOnes_RequiresSigningIn()
    {
        HttpClient visitor = TestAccounts.Browser(factory);
        List<(string Method, string Route)> endpoints =
        [
            .. factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
                .OfType<RouteEndpoint>()
                .Where(e => e.RoutePattern.RawText!.StartsWith("/api/", StringComparison.Ordinal)
                    && !e.RoutePattern.RawText.StartsWith("/api/auth/", StringComparison.Ordinal))
                .SelectMany(e => e.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods
                    .Select(method => (method, e.RoutePattern.RawText!))),
        ];

        // Recipes (5), tags (1) and shopping lists (10): a new endpoint is checked as well.
        Assert.True(endpoints.Count >= 16, $"Only {endpoints.Count} endpoints found.");
        foreach ((string method, string route) in endpoints)
        {
            using HttpRequestMessage request = new(new HttpMethod(method), RouteParameter().Replace(route, Guid.NewGuid().ToString()));
            if (method != "GET")
            {
                request.Content = JsonContent.Create(new { });
            }

            HttpResponseMessage response = await visitor.SendAsync(request);

            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized, $"{method} {route}: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task ApiWrite_WithoutAntiforgeryToken_Returns400AndChangesNothing()
    {
        HttpClient client = TestAccounts.WithoutAntiforgery(factory);
        string email = TestAccounts.NewEmail();
        string? token = TestAccounts.AntiforgeryToken(await client.GetAsync("/api/auth/session"));
        using HttpRequestMessage register = new(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new Features.Accounts.RegisterInput(email, TestAccounts.Password)),
        };
        register.Headers.Add("X-XSRF-TOKEN", token);
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(register)).StatusCode);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/shopping-lists", new ShoppingListInput("Forged"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Page<ListShoppingLists.ShoppingListSummary>? lists =
            await client.GetFromJsonAsync<Page<ListShoppingLists.ShoppingListSummary>>("/api/shopping-lists");
        Assert.Empty(lists!.Items);
    }

    [Fact]
    public async Task ApiResponses_AreNotKeptInTheBrowserHttpCache()
    {
        HttpResponseMessage response = await _alice.GetAsync("/api/recipes");

        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task DataWithoutOwner_GoesToTheFirstAccountOnly()
    {
        // A database of its own: "first account" must mean the first one in it.
        await using RecipeBookApiFactory fresh = new();
        await AddWithoutOwnerAsync(fresh, "From before accounts");
        HttpClient first = TestAccounts.SignedIn(fresh);
        HttpClient second = TestAccounts.SignedIn(fresh);

        Page<ListRecipes.RecipeSummary>? firsts = await first.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>("/api/recipes");
        Page<ListShoppingLists.ShoppingListSummary>? firstLists =
            await first.GetFromJsonAsync<Page<ListShoppingLists.ShoppingListSummary>>("/api/shopping-lists");
        Page<ListRecipes.RecipeSummary>? seconds = await second.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>("/api/recipes");

        Assert.Equal(["From before accounts"], firsts!.Items.Select(r => r.Title));
        Assert.Equal(["From before accounts"], firstLists!.Items.Select(l => l.Name));
        Assert.Empty(seconds!.Items);

        // Once accounts exist, nobody takes what is added without an owner.
        await AddWithoutOwnerAsync(fresh, "Later, without owner");
        HttpClient third = TestAccounts.SignedIn(fresh);
        Assert.Empty((await third.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>("/api/recipes"))!.Items);
        Assert.DoesNotContain(
            (await first.GetFromJsonAsync<Page<ListRecipes.RecipeSummary>>("/api/recipes"))!.Items,
            r => r.Title == "Later, without owner");
    }

    private static async Task AddWithoutOwnerAsync(RecipeBookApiFactory factory, string name)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DateTime now = DateTime.UtcNow;
        db.Recipes.Add(new Recipe { Title = name, CreatedAt = now, UpdatedAt = now });
        db.ShoppingLists.Add(new ShoppingList { Name = name, CreatedAt = now, UpdatedAt = now });
        await db.SaveChangesAsync();
        Assert.Equal(1, await db.Recipes.IgnoreQueryFilters().CountAsync(r => r.Title == name && r.OwnerId == null));
    }

    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex RouteParameter();
}
