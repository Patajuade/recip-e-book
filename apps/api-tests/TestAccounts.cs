using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.DependencyInjection;

using RecipEBook.Api.Features.Accounts;

namespace RecipEBook.Api.IntegrationTests;

/// <summary>Clients that behave like the browser: HTTPS, cookies, and the antiforgery header.</summary>
internal static class TestAccounts
{
    public const string Password = "a long enough passphrase";

    public static string NewEmail() => $"{Guid.NewGuid():N}@example.com";

    /// <summary>Sends the antiforgery token on writes, as Angular's HttpClient does.</summary>
    public static HttpClient Browser(RecipeBookApiFactory factory) =>
        factory.CreateDefaultClient(new Uri("https://localhost"), new AntiforgeryHandler(), new CookieContainerHandler());

    /// <summary>
    /// A browser with its own new account, created on its first request, so a test class's
    /// constructor can set it up without awaiting.
    /// </summary>
    public static HttpClient SignedIn(RecipeBookApiFactory factory) =>
        factory.CreateDefaultClient(
            new Uri("https://localhost"), new SignUpHandler(), new AntiforgeryHandler(), new CookieContainerHandler());

    /// <summary>The id of the account a client is signed in with.</summary>
    public static async Task<string> UserIdAsync(RecipeBookApiFactory factory, HttpClient client)
    {
        string email = (await SignedInEmailAsync(client))!;
        using IServiceScope scope = factory.Services.CreateScope();
        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        return (await users.FindByEmailAsync(email))!.Id;
    }

    /// <summary>Keeps cookies but sends no antiforgery token.</summary>
    public static HttpClient WithoutAntiforgery(RecipeBookApiFactory factory) =>
        factory.CreateDefaultClient(new Uri("https://localhost"), new CookieContainerHandler());

    public static async Task RegisterAsync(HttpClient client, string email, string password = Password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new RegisterInput(email, password));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public static async Task<string?> SignedInEmailAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<GetSession.Session>("/api/auth/session"))!.Email;

    /// <summary>Reads the token the session endpoint writes as a cookie, URL-decoded as Angular does.</summary>
    public static string? AntiforgeryToken(HttpResponseMessage session) =>
        session.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies)
            ? cookies
                .Where(cookie => cookie.StartsWith($"{AccountSetup.AntiforgeryCookie}=", StringComparison.Ordinal))
                .Select(cookie => Uri.UnescapeDataString(cookie.Split(';')[0].Split('=', 2)[1]))
                .FirstOrDefault()
            : null;

    private sealed class SignUpHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _once = new(1, 1);
        private bool _signedUp;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await _once.WaitAsync(cancellationToken);
            try
            {
                if (!_signedUp)
                {
                    using HttpRequestMessage register = new(HttpMethod.Post, new Uri(request.RequestUri!, "/api/auth/register"))
                    {
                        Content = JsonContent.Create(new RegisterInput(NewEmail(), Password)),
                    };
                    using HttpResponseMessage response = await base.SendAsync(register, cancellationToken);
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    _signedUp = true;
                }
            }
            finally
            {
                _once.Release();
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    // The token depends on the user, so it is fetched again before each write.
    private sealed class AntiforgeryHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
            {
                using HttpRequestMessage sessionRequest = new(HttpMethod.Get, new Uri(request.RequestUri!, "/api/auth/session"));
                using HttpResponseMessage session = await base.SendAsync(sessionRequest, cancellationToken);
                request.Headers.Add("X-XSRF-TOKEN", AntiforgeryToken(session));
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
