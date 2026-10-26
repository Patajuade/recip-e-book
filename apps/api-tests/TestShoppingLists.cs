using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests;

/// <summary>Creates shopping lists straight in the database, for the tests that only read them.</summary>
internal static class TestShoppingLists
{
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
