using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Features.ShoppingLists;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests;

/// <summary>Creates shopping lists through the API, or straight in the database with given items.</summary>
internal static class TestShoppingLists
{
    public static async Task<Guid> CreateAsync(HttpClient client, string name = "Week")
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/shopping-lists", new ShoppingListInput(name));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CreateShoppingList.Response? body = await response.Content.ReadFromJsonAsync<CreateShoppingList.Response>();
        return body!.Id;
    }

    public static async Task<Guid> AddAsync(
        RecipeBookApiFactory factory, string name, DateTime updatedAt, params ShoppingListItem[] items)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ShoppingList list = new() { Name = name, CreatedAt = updatedAt, UpdatedAt = updatedAt };
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
