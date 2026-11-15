using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Features.Accounts;
using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.ShoppingLists;
using RecipEBook.Api.Features.Tags;
using RecipEBook.Api.Infrastructure.Persistence;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// The build-time OpenAPI generation runs this file without the connection string. The
// DbContext stays registered, so the endpoints still see it as a service; it is never opened.
bool generatingOpenApiDocument = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
var connectionString = builder.Configuration.GetConnectionString("RecipEBook")
    ?? (generatingOpenApiDocument
        ? "Host=localhost"
        : throw new InvalidOperationException("Connection string 'RecipEBook' not found."));
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        // Demo recipes, inserted by `dotnet ef database update` (sync) or MigrateAsync (async).
        options
            .UseSeeding((context, _) => DemoData.Seed(context))
            .UseAsyncSeeding((context, _, cancellationToken) => DemoData.SeedAsync(context, cancellationToken));
    }
});

// The web defaults also accept numbers written as strings ("4"), so the OpenAPI document typed
// every integer as integer or string. Numbers are numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddHealthChecks();
builder.Services.AddAccounts(keysInDatabase: !generatingOpenApiDocument);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = ["en", "fr"];
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

// First, so every later middleware and endpoint sees the request culture (Accept-Language).
app.UseRequestLocalization();
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // In Development, an invalid request (malformed JSON…) throws instead of answering 400
    // (RouteHandlerOptions.ThrowOnBadRequest): keep its status code rather than a 500.
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// In production the API also serves the Angular build, copied to wwwroot by the Dockerfile, so
// the app and /api share one origin. In development the Angular dev server does it instead.
bool servesWebApp = app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists;
StaticFileOptions webAppFiles = new()
{
    // Revalidate every time (ETag): index.html and ngsw.json must never be served stale, or the
    // service worker would miss new versions.
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache",
};
if (servesWebApp)
{
    // "/" is served as index.html by the static files middleware, not by the fallback below.
    app.UseDefaultFiles();
    app.UseStaticFiles(webAppFiles);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

// Every write of the API proves it comes from the app (antiforgery), and everything but the
// accounts endpoints needs a signed-in user.
RouteGroupBuilder api = app.MapGroup("/api").RequireAntiforgeryToken();
api.MapAccountEndpoints();
RouteGroupBuilder signedIn = api.MapGroup("").RequireAuthorization().AddEndpointFilter((context, next) =>
{
    // Personal data: the browser's HTTP cache must not keep it for the next user of the device.
    context.HttpContext.Response.Headers.CacheControl = "no-store";
    return next(context);
});
signedIn.MapRecipeEndpoints();
signedIn.MapTagEndpoints();
signedIn.MapShoppingListEndpoints();

if (servesWebApp)
{
    // The app's routes (/recipes/…) get index.html, as Angular's deployment guide asks. Not under
    // /api: an unknown API route stays a 404 instead of answering a page.
    app.MapFallbackToFile("{*path:nonfile:regex(^(?!api(/|$)))}", "index.html", webAppFiles);
}

app.Run();
