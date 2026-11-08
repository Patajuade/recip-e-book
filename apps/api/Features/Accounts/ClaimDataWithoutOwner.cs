using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.Accounts;

/// <summary>
/// Recipes and lists from before accounts have no owner, and nobody sees them. The first
/// account created takes them (developer's choice); a database that already has accounts
/// keeps them hidden.
/// </summary>
public static class ClaimDataWithoutOwner
{
    public static async Task ForFirstAccountAsync(AppDbContext db, string userId, CancellationToken cancellationToken)
    {
        if (await db.Users.CountAsync(cancellationToken) != 1)
        {
            return;
        }

        await db.Recipes
            .IgnoreQueryFilters([AppDbContext.OwnerFilter])
            .Where(r => r.OwnerId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.OwnerId, userId), cancellationToken);
        await db.ShoppingLists
            .IgnoreQueryFilters([AppDbContext.OwnerFilter])
            .Where(l => l.OwnerId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.OwnerId, userId), cancellationToken);
    }
}
