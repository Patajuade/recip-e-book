using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipEBook.Api.Common;
using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence;

// IdentityUserContext: the account tables, without the roles the app has no use for.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, CurrentUser currentUser)
    : IdentityUserContext<IdentityUser>(options)
{
    /// <summary>Name of the query filter that keeps each user to their own data.</summary>
    public const string OwnerFilter = "Owner";

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }

    // Read by the query filters at each query (EF Core parameterizes context members).
    private string? CurrentUserId => currentUser.Id;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Every query sees the signed-in user's data only, so a new endpoint can't forget it.
        // The null check fails closed: without a user, rows without an owner stay hidden too.
        // A list's items are only reached through their list, which is filtered.
        modelBuilder.Entity<Recipe>().HasQueryFilter(
            OwnerFilter, r => r.OwnerId != null && r.OwnerId == CurrentUserId);
        modelBuilder.Entity<ShoppingList>().HasQueryFilter(
            OwnerFilter, l => l.OwnerId != null && l.OwnerId == CurrentUserId);
    }
}
