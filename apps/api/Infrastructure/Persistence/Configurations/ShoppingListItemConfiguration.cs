using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence.Configurations;

public sealed class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{
    public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
    {
        // The default for an optional relationship, ClientSetNull, only fixes loaded items:
        // recipes are deleted with ExecuteDeleteAsync, so the database must null the link.
        builder.HasOne(i => i.SourceRecipe)
            .WithMany()
            .HasForeignKey(i => i.SourceRecipeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
