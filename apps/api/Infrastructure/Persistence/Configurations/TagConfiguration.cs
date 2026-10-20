using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Key);
        builder.Property(t => t.Category).HasConversion<string>();
        builder.HasData(TagCatalog.Tags.Select(t => new Tag { Key = t.Key, Category = t.Category }));
    }
}
