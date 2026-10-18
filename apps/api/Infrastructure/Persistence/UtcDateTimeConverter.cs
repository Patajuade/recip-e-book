using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RecipEBook.Api.Infrastructure.Persistence;

/// <summary>
/// All dates are stored in UTC. SQLite stores them as text and doesn't keep
/// DateTime.Kind, so values read back are marked as UTC again; otherwise they
/// would be serialized without the "Z" suffix and read as local time by clients.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.ToUniversalTime(),
            v => new DateTime(v.Ticks, DateTimeKind.Utc))
    {
    }
}
