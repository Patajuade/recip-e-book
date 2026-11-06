using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace RecipEBook.Api.Features.Accounts;

public record LoginInput([Required, StringLength(256)] string Email, [Required, StringLength(128)] string Password);

/// <summary>Command — POST /api/auth/login: signs in with an email and a password.</summary>
public static class Login
{
    public static async Task<Results<NoContent, ProblemHttpResult>> Handle(
        LoginInput input,
        SignInManager<IdentityUser> signIn,
        IStringLocalizer<AccountMessages> localizer)
    {
        SignInResult result = await signIn.PasswordSignInAsync(
            input.Email, input.Password, isPersistent: true, lockoutOnFailure: true);

        // One answer for an unknown email, a wrong password and a locked account (OWASP), so it
        // doesn't reveal which emails have an account.
        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: localizer["Wrong email or password, or too many attempts. Try again in a few minutes."]);
    }
}
