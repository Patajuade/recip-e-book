using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace RecipEBook.Api.Features.Accounts;

/// <summary>Marks the resource file of the accounts messages.</summary>
public sealed class AccountMessages;

/// <summary>Identity's own messages are English only. Only the ones these settings can produce.</summary>
public sealed class LocalizedIdentityErrors(IStringLocalizer<AccountMessages> localizer) : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length) =>
        new()
        {
            Code = nameof(PasswordTooShort),
            Description = localizer["The password must have at least {0} characters.", length],
        };

    // The user name is the email, so both errors say the same thing.
    public override IdentityError DuplicateEmail(string email) => EmailTaken(nameof(DuplicateEmail));

    public override IdentityError DuplicateUserName(string userName) => EmailTaken(nameof(DuplicateUserName));

    private IdentityError EmailTaken(string code) =>
        new() { Code = code, Description = localizer["An account already uses this email."] };
}
