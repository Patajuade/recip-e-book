using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests.Recipes;

public class DeleteRecipeTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Delete_ExistingRecipe_Returns204ThenTheRecipeIsGone()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());

        HttpResponseMessage response = await _client.DeleteAsync($"/api/recipes/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/recipes/{id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_Twice_Returns404TheSecondTime()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid());
        await _client.DeleteAsync($"/api/recipes/{id}");

        HttpResponseMessage response = await _client.DeleteAsync($"/api/recipes/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesTheTagLinksButKeepsTheCatalog()
    {
        Guid id = await TestRecipes.CreateAsync(_client, TestRecipes.Valid("Linked", "soup", "winter"));

        await _client.DeleteAsync($"/api/recipes/{id}");

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int links = await db.Database
            .SqlQuery<int>($"""SELECT COUNT(*) AS "Value" FROM "RecipeTag" WHERE "RecipesId" = {id}""")
            .SingleAsync();
        Assert.Equal(0, links);
        Assert.Equal(101, await db.Tags.CountAsync());
    }
}
