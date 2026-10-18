using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence.Configurations;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        // Complex types aren't discovered by convention; on relational providers,
        // complex collections must be mapped to a JSON column.
        builder.ComplexCollection(r => r.Ingredients, b => b.ToJson());
        builder.ComplexCollection(r => r.Steps, b => b.ToJson());
    }
}
