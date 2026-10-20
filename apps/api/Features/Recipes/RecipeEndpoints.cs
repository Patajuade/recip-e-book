namespace RecipEBook.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public static RouteGroupBuilder MapRecipeEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder recipes = api.MapGroup("/recipes").WithTags("Recipes");

        recipes.MapGet("/", ListRecipes.Handle).WithName("ListRecipes");
        recipes.MapGet("/{id:guid}", GetRecipe.Handle).WithName("GetRecipe");
        recipes.MapPost("/", CreateRecipe.Handle).WithName("CreateRecipe").ProducesValidationProblem();
        recipes.MapPut("/{id:guid}", UpdateRecipe.Handle).WithName("UpdateRecipe").ProducesValidationProblem();

        return api;
    }
}
