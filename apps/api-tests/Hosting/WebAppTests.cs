using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.IntegrationTests.Hosting;

/// <summary>The API with a web root holding a stand-in Angular build, as in the container image.</summary>
public sealed class WebAppFixture : IDisposable
{
    public const string IndexMarker = "<app-root></app-root>";

    private readonly string _webRoot = Directory.CreateTempSubdirectory("recip-e-book-web-").FullName;
    private readonly RecipeBookApiFactory _api = new();

    public WebAppFixture()
    {
        File.WriteAllText(Path.Combine(_webRoot, "index.html"), $"<!doctype html><html><body>{IndexMarker}</body></html>");
        File.WriteAllText(Path.Combine(_webRoot, "main-TEST1234.js"), "console.log('app');");
        Factory = _api.WithWebHostBuilder(builder => builder.UseWebRoot(_webRoot));
    }

    public WebApplicationFactory<Program> Factory { get; }

    public HttpClient Client() => Factory.CreateDefaultClient(new Uri("https://localhost"));

    public void Dispose()
    {
        Factory.Dispose();
        _api.Dispose();
        Directory.Delete(_webRoot, recursive: true);
    }
}

public class WebAppTests(WebAppFixture web) : IClassFixture<WebAppFixture>
{
    [Theory]
    [InlineData("/")]
    [InlineData("/recipes")]
    [InlineData("/recipes/0b6f3c1e-6a43-4c55-9d38-0c5e8d1f2a7b/edit")]
    [InlineData("/apiary")]
    public async Task AppRoutes_GetIndexHtml_RevalidatedEachTime(string path)
    {
        HttpResponseMessage response = await web.Client().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(WebAppFixture.IndexMarker, await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task BuildFiles_AreServed_RevalidatedEachTime()
    {
        HttpResponseMessage response = await web.Client().GetAsync("/main-TEST1234.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task MissingFile_IsNotFound()
    {
        HttpResponseMessage response = await web.Client().GetAsync("/chunk-MISSING1.js");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api")]
    [InlineData("/api/unknown")]
    [InlineData("/api/recipes/not-a-guid")]
    public async Task UnknownApiRoutes_AreNotFound_NotThePage(string path)
    {
        HttpResponseMessage response = await web.Client().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(WebAppFixture.IndexMarker, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Api_StillRequiresSignIn()
    {
        HttpResponseMessage response = await web.Client().GetAsync("/api/recipes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class WithoutWebAppTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    [Fact]
    public async Task WithoutBuild_AppRoutesAreNotFound()
    {
        HttpResponseMessage response = await factory.CreateDefaultClient(new Uri("https://localhost")).GetAsync("/recipes");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CookieKeys_AreKeptInTheDatabase()
    {
        // Issuing the antiforgery token needs a Data Protection key.
        await TestAccounts.Browser(factory).GetAsync("/api/auth/session");

        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.NotEmpty(await db.DataProtectionKeys.ToListAsync());
    }
}
