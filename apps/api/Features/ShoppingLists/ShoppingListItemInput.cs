using System.ComponentModel.DataAnnotations;

using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>
/// Write model of an item, to add or update it. No id, list or position: the server takes
/// them from the route and the list.
/// </summary>
public record ShoppingListItemInput(
    [Required, StringLength(200)] string Name,
    [Range(0d, 100_000d)] decimal? Quantity = null,
    [StringLength(50)] string? Unit = null,
    bool Checked = false)
{
    internal void ApplyTo(ShoppingListItem item)
    {
        item.Name = Name.Trim();
        item.Quantity = Quantity;
        item.Unit = string.IsNullOrWhiteSpace(Unit) ? null : Unit.Trim();
        item.Checked = Checked;
    }
}
