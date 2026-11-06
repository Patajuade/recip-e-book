using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace RecipEBook.Api.Features.Accounts;

// The upper limit on the password stops very long ones from being sent to the hash function.
public record RegisterInput(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);

/// <summary>
/// Command — POST /api/auth/register: creates an account and signs in, while registration is
/// open (the `Accounts:RegistrationOpen` setting, closed when missing).
/// </summary>
public static class Register
{
    public static bool IsOpen(IConfiguration configuration) =>
        configuration.GetValue<bool>("Accounts:RegistrationOpen");

    public static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> Handle(
        RegisterInput input,
        UserManager<IdentityUser> users,
        SignInManager<IdentityUser> signIn,
        IConfiguration configuration,
        IStringLocalizer<AccountMessages> localizer)
    {
        if (!IsOpen(configuration))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: localizer["Registration is closed."]);
        }

        IdentityUser user = new() { UserName = input.Email, Email = input.Email };
        IdentityResult result = await users.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(ToFieldErrors(result.Errors));
        }

        await signIn.SignInAsync(user, isPersistent: true);
        return TypedResults.NoContent();
    }

    private static Dictionary<string, string[]> ToFieldErrors(IEnumerable<IdentityError> errors) =>
        errors
            .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal)
                ? nameof(RegisterInput.Password)
                : nameof(RegisterInput.Email))
            .ToDictionary(
                field => field.Key,
                field => field.Select(error => error.Description).Distinct().ToArray());
}
