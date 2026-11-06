using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

using RecipEBook.Api.Features.Accounts;

namespace RecipEBook.Api.IntegrationTests.Accounts;

public class AccountTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    private readonly HttpClient _client = TestAccounts.Browser(factory);

    [Fact]
    public async Task Session_NotSignedIn_HasNoEmail()
    {
        GetSession.Session? session = await _client.GetFromJsonAsync<GetSession.Session>("/api/auth/session");

        Assert.Null(session!.Email);
        Assert.True(session.RegistrationOpen);
    }

    [Fact]
    public async Task Register_CreatesTheAccountAndSignsIn()
    {
        string email = TestAccounts.NewEmail();

        await TestAccounts.RegisterAsync(_client, email);

        Assert.Equal(email, await TestAccounts.SignedInEmailAsync(_client));
    }

    [Fact]
    public async Task Register_AcceptsAnApostropheInTheEmail()
    {
        await TestAccounts.RegisterAsync(_client, $"o'neil-{Guid.NewGuid():N}@example.com");
    }

    [Fact]
    public async Task Register_EmailAlreadyUsed_Returns400WithOneMessage()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);

        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/auth/register", new RegisterInput(email, TestAccounts.Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["An account already uses this email."], problem!.Errors["Email"]);
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400UnderThePassword()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterInput(TestAccounts.NewEmail(), "fourteen chars"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["The password must have at least 15 characters."], problem!.Errors["Password"]);
    }

    [Fact]
    public async Task Register_LongPasswordWithoutSymbolsOrDigits_IsAccepted()
    {
        await TestAccounts.RegisterAsync(_client, TestAccounts.NewEmail(), "only lowercase words here");
    }

    [Theory]
    [InlineData("not-an-email", TestAccounts.Password)]
    [InlineData("someone@example.com", "")]
    public async Task Register_InvalidInput_Returns400(string email, string password)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterInput(email, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordOver128Characters_Returns400()
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterInput(TestAccounts.NewEmail(), new string('x', 129)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_InFrench_ReturnsFrenchMessages()
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new RegisterInput(TestAccounts.NewEmail(), "short")),
        };
        request.Headers.AcceptLanguage.ParseAdd("fr");

        HttpResponseMessage response = await _client.SendAsync(request);

        HttpValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Equal(["Le mot de passe doit contenir au moins 15 caractères."], problem!.Errors["Password"]);
    }

    [Fact]
    public async Task Register_WhenClosed_Returns403AndCreatesNothing()
    {
        // The same in-memory database, with the setting off.
        WebApplicationFactory<Program> closed = factory.WithWebHostBuilder(
            builder => builder.UseSetting("Accounts:RegistrationOpen", "false"));
        HttpClient client = closed.CreateDefaultClient(
            new Uri("https://localhost"), new Microsoft.AspNetCore.Mvc.Testing.Handlers.CookieContainerHandler());
        HttpResponseMessage session = await client.GetAsync("/api/auth/session");
        string email = TestAccounts.NewEmail();
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new RegisterInput(email, TestAccounts.Password)),
        };
        request.Headers.Add("X-XSRF-TOKEN", TestAccounts.AntiforgeryToken(session));

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.False((await session.Content.ReadFromJsonAsync<GetSession.Session>())!.RegistrationOpen);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        HttpResponseMessage login = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginInput(email, TestAccounts.Password));
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Register_IsClosedOutsideDevelopment_WhenTheSettingIsMissing()
    {
        WebApplicationFactory<Program> production = factory.WithWebHostBuilder(builder => builder
            .UseEnvironment("Production")
            .UseSetting("ConnectionStrings:RecipEBook", "Data Source=:memory:"));
        HttpClient client = production.CreateClient();

        GetSession.Session? session = await client.GetFromJsonAsync<GetSession.Session>("https://localhost/api/auth/session");

        Assert.False(session!.RegistrationOpen);
    }

    [Fact]
    public async Task Login_RightPassword_SignsIn()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);

        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, TestAccounts.Password));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(email, await TestAccounts.SignedInEmailAsync(_client));
    }

    [Fact]
    public async Task Login_UnknownEmailAndWrongPassword_GetTheSameAnswer()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);

        HttpResponseMessage wrongPassword =
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, "not the right passphrase"));
        HttpResponseMessage unknownEmail =
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(TestAccounts.NewEmail(), TestAccounts.Password));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(wrongPassword.StatusCode, unknownEmail.StatusCode);
        Assert.Equal(
            (await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail,
            (await unknownEmail.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail);
        Assert.Null(await TestAccounts.SignedInEmailAsync(_client));
    }

    [Fact]
    public async Task Login_AfterFiveFailures_RefusesEvenTheRightPassword()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);
        for (int attempt = 0; attempt < 5; attempt++)
        {
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, "not the right passphrase"));
        }

        HttpResponseMessage response =
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, TestAccounts.Password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(await TestAccounts.SignedInEmailAsync(_client));
    }

    [Fact]
    public async Task Logout_SignsOut()
    {
        await TestAccounts.RegisterAsync(_client, TestAccounts.NewEmail());

        HttpResponseMessage response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await TestAccounts.SignedInEmailAsync(_client));
    }

    [Fact]
    public async Task Logout_NotSignedIn_Returns401()
    {
        HttpResponseMessage response = await TestAccounts.Browser(factory).PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Write_WithoutAntiforgeryToken_Returns400AndDoesNothing()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);
        HttpClient client = TestAccounts.WithoutAntiforgery(factory);

        HttpResponseMessage response =
            await client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, TestAccounts.Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(await TestAccounts.SignedInEmailAsync(client));
    }

    [Fact]
    public async Task Write_WithATokenFromBeforeSigningIn_Returns400()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);
        HttpClient client = TestAccounts.WithoutAntiforgery(factory);
        string? anonymousToken = TestAccounts.AntiforgeryToken(await client.GetAsync("/api/auth/session"));
        using HttpRequestMessage login = new(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginInput(email, TestAccounts.Password)),
        };
        login.Headers.Add("X-XSRF-TOKEN", anonymousToken);
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(login)).StatusCode);

        using HttpRequestMessage logout = new(HttpMethod.Post, "/api/auth/logout");
        logout.Headers.Add("X-XSRF-TOKEN", anonymousToken);
        HttpResponseMessage response = await client.SendAsync(logout);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(email, await TestAccounts.SignedInEmailAsync(client));
    }

    [Fact]
    public async Task Cookies_HaveTheirSecurityAttributes()
    {
        string email = TestAccounts.NewEmail();
        await TestAccounts.RegisterAsync(TestAccounts.Browser(factory), email);

        HttpResponseMessage login =
            await _client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, TestAccounts.Password));
        HttpResponseMessage session = await _client.GetAsync("/api/auth/session");

        string authentication = Assert.Single(login.Headers.GetValues("Set-Cookie"), c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.Contains("httponly", authentication, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", authentication, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", authentication, StringComparison.OrdinalIgnoreCase);

        // Readable by the app: that is the point of this one.
        string token = Assert.Single(session.Headers.GetValues("Set-Cookie"), c => c.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        Assert.DoesNotContain("httponly", token, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", token, StringComparison.OrdinalIgnoreCase);
    }
}
