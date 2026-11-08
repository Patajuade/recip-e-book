using System.Security.Claims;

namespace RecipEBook.Api.Common;

/// <summary>The signed-in user of the current request.</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor)
{
    /// <summary>Null outside a request (seeding, migrations) or when nobody is signed in.</summary>
    public string? Id => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>For endpoints that require a signed-in user, so the id is always there.</summary>
    public string RequiredId => Id ?? throw new InvalidOperationException("No signed-in user.");
}
