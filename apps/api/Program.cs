using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

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
        ? "Data Source=:memory:"
        : throw new InvalidOperationException("Connection string 'RecipEBook' not found."));
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);

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

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

RouteGroupBuilder api = app.MapGroup("/api");
api.MapRecipeEndpoints();
api.MapTagEndpoints();
api.MapShoppingListEndpoints();

app.Run();
