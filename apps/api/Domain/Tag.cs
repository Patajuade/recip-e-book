namespace RecipEBook.Api.Domain;

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<Recipe> Recipes { get; } = [];
}