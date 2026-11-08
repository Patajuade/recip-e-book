using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence.Configurations;

public sealed class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
{
    public void Configure(EntityTypeBuilder<ShoppingList> builder)
    {
        // Items are their own table, not a JSON column like recipe ingredients: they are
        // checked one by one, so each update touches one row.
        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        // The owner is an account; the domain doesn't know Identity, only this configuration does.
        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(l => l.OwnerId).OnDelete(DeleteBehavior.Cascade);

        // Matches the list's query: one owner, in paging order (EF Core pagination guide).
        builder.HasIndex(l => new { l.OwnerId, l.UpdatedAt, l.Id });
    }
}
