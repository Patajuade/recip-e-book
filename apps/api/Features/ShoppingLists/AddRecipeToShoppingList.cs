using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using RecipEBook.Api.Domain;
using RecipEBook.Api.Infrastructure.Persistence;

namespace RecipEBook.Api.Features.ShoppingLists;

/// <summary>Body of POST /api/shopping-lists/{id}/recipes: which recipe, for how many servings.</summary>
public record AddRecipeInput(Guid RecipeId, [Range(1, 100)] int? Servings = null);

/// <summary>
/// Command — POST /api/shopping-lists/{id}/recipes: copies a recipe's ingredients at the end
/// of the list, scaled to <see cref="AddRecipeInput.Servings"/> when both it and the recipe's
/// servings are known; returns 200 with how many items were added. The server reads the
/// ingredients itself: the client only names the recipe. 404 if the list doesn't exist,
/// 400 if the recipe doesn't.
/// </summary>
public static class AddRecipeToShoppingList
{
    public record Response(int ItemsAdded);

    public static async Task<Results<Ok<Response>, NotFound, ValidationProblem>> Handle(
        Guid id,
        AddRecipeInput input,
        AppDbContext db,
        IStringLocalizer<AddRecipeInput> localizer,
        CancellationToken cancellationToken)
    {
        ShoppingList? list = await db.ShoppingLists.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (list is null)
        {
            return TypedResults.NotFound();
        }

        var recipe = await db.Recipes
            .Where(r => r.Id == input.RecipeId)
            .Select(r => new { r.Id, r.Servings, r.Ingredients })
            .SingleOrDefaultAsync(cancellationToken);
        if (recipe is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(AddRecipeInput.RecipeId)] = [localizer["Unknown recipe."]],
            });
        }

        decimal? factor = input.Servings is int wanted && recipe.Servings is int planned
            ? (decimal)wanted / planned
            : null;
        int position = await AddShoppingListItem.NextPositionAsync(db, id, cancellationToken);
        list.Items.AddRange(recipe.Ingredients.Select((ingredient, index) => new ShoppingListItem
        {
            Name = ingredient.Name,
            Quantity = ingredient.Quantity is decimal quantity && factor is decimal f
                ? Math.Round(quantity * f, 2)
                : ingredient.Quantity,
            Unit = ingredient.Unit,
            Position = position + index,
            SourceRecipeId = recipe.Id,
        }));
        list.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new Response(recipe.Ingredients.Count));
    }
}
