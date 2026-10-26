namespace RecipEBook.Api.Domain;

/// <summary>
/// One thing to buy. An item added from a recipe is a copy of an ingredient: editing or
/// deleting the recipe doesn't change it; <see cref="SourceRecipeId"/> only links back.
/// </summary>
public class ShoppingListItem
{
    public Guid Id { get; set; }
    public Guid ShoppingListId { get; set; }
    public required string Name { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public bool Checked { get; set; }

    /// <summary>Place in the list, from 0.</summary>
    public int Position { get; set; }

    /// <summary>The recipe it was copied from; null for a free item, or once that recipe is deleted.</summary>
    public Guid? SourceRecipeId { get; set; }
    public Recipe? SourceRecipe { get; set; }
}
