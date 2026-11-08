using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

using RecipEBook.Api.Common;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Accounts;

public static class AccountSetup
{
    /// <summary>The name Angular's HttpClient reads the antiforgery token from.</summary>
    public const string AntiforgeryCookie = "XSRF-TOKEN";

    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddAuthorization();

        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // The user name is the email: the default list of allowed characters would
                // refuse valid addresses such as o'neil@example.com.
                options.User.AllowedUserNameCharacters = "";

                // OWASP: shorter than 15 characters is weak without MFA, and no composition rules.
                options.Password.RequiredLength = 15;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddErrorDescriber<LocalizedIdentityErrors>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    /// <summary>
    /// Refuses writes without a valid antiforgery token. Minimal APIs only check it by themselves
    /// for form data, and never for DELETE, and these endpoints read JSON.
    /// </summary>
    public static TBuilder RequireAntiforgeryToken<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            HttpContext http = context.HttpContext;
            IAntiforgery antiforgery = http.RequestServices.GetRequiredService<IAntiforgery>();
            if (await antiforgery.IsRequestValidAsync(http))
            {
                return await next(context);
            }

            IStringLocalizer<AccountMessages> localizer =
                http.RequestServices.GetRequiredService<IStringLocalizer<AccountMessages>>();
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: localizer["The request could not be verified. Reload the page and try again."]);
        });
        return builder;
    }
}
