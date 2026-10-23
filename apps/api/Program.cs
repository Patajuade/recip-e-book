using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Features.Recipes;
using RecipEBook.Api.Features.Tags;
using RecipEBook.Api.Infrastructure.Persistence;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RecipEBook")
    ?? throw new InvalidOperationException("Connection string 'RecipEBook' not found.");
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
app.UseExceptionHandler();
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

app.Run();
