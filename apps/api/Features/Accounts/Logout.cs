using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace RecipEBook.Api.Features.Accounts;

/// <summary>Command — POST /api/auth/logout: signs out.</summary>
public static class Logout
{
    public static async Task<NoContent> Handle(SignInManager<IdentityUser> signIn)
    {
        await signIn.SignOutAsync();
        return TypedResults.NoContent();
    }
}
