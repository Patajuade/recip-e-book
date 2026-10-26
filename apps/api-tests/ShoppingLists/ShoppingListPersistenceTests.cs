using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests.ShoppingLists;

/// <summary>The delete behaviors of the shopping list tables, enforced by the database.</summary>
public class ShoppingListPersistenceTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task DeletingARecipe_KeepsTheItemsCopiedFromItWithoutTheLink()
    {
        Guid recipeId = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Source"));
        Guid listId = await AddListAsync(new ShoppingListItem { Name = "carrots", Quantity = 3, SourceRecipeId = recipeId });

        HttpResponseMessage response = await _client.DeleteAsync($"/api/recipes/{recipeId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ShoppingListItem item = await db.ShoppingLists
            .Where(l => l.Id == listId)
            .SelectMany(l => l.Items)
            .SingleAsync();
        Assert.Equal("carrots", item.Name);
        Assert.Equal(3, item.Quantity);
        Assert.Null(item.SourceRecipeId);
    }

    [Fact]
    public async Task DeletingAList_DeletesItsItems()
    {
        Guid listId = await AddListAsync(new ShoppingListItem { Name = "bread" }, new ShoppingListItem { Name = "milk" });

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Items aren't loaded: only the database cascade can delete them.
        await db.ShoppingLists.Where(l => l.Id == listId).ExecuteDeleteAsync();

        int items = await db.Database
            .SqlQuery<int>($"""SELECT COUNT(*) AS "Value" FROM "ShoppingListItems" WHERE "ShoppingListId" = {listId}""")
            .SingleAsync();
        Assert.Equal(0, items);
    }

    private async Task<Guid> AddListAsync(params ShoppingListItem[] items)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DateTime now = DateTime.UtcNow;
        ShoppingList list = new() { Name = "Test", CreatedAt = now, UpdatedAt = now };
        list.Items.AddRange(items.Select((item, position) =>
        {
            item.Position = position;
            return item;
        }));
        db.ShoppingLists.Add(list);
        await db.SaveChangesAsync();
        return list.Id;
    }
}
