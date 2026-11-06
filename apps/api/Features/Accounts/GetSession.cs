using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RecipEBook.Api.Features.Accounts;

/// <summary>
/// Query — GET /api/auth/session: who is signed in and whether accounts can be created. Writes
/// the antiforgery token as a cookie the app can read; the token depends on the user, so the
/// app calls this again after signing in or out.
/// </summary>
public static class GetSession
{
    public record Session(string? Email, bool RegistrationOpen);

    public static Ok<Session> Handle(HttpContext context, IAntiforgery antiforgery, IConfiguration configuration)
    {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append(
            AccountSetup.AntiforgeryCookie,
            tokens.RequestToken!,
            new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.Strict });

        return TypedResults.Ok(new Session(
            context.User.Identity?.Name,
            Register.IsOpen(configuration)));
    }
}
