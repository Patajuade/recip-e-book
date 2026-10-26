namespace RecipEBook.Api.Domain;

/// <summary>A named shopping list (« Semaine », « Anniversaire »…) and the things to buy.</summary>
public class ShoppingList
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ShoppingListItem> Items { get; } = [];
}
