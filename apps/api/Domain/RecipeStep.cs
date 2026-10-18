namespace RecipEBook.Api.Domain;

public record RecipeStep
{
    public required string Instruction { get; init; }
}