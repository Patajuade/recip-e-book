namespace RecipEBook.Api.Domain;

public record Ingredient
{
    public required string Name { get; init; }
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
}