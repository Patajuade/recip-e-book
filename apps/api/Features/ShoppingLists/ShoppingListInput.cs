using System.ComponentModel.DataAnnotations;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>Write model to create or rename a list: only its name, the server sets the rest.</summary>
public record ShoppingListInput([Required, StringLength(100)] string Name);
