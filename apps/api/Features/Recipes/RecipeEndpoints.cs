namespace RecipEBook.Api.Features.Recipes;

public static class RecipeEndpoints
{
    public static RouteGroupBuilder MapRecipeEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder recipes = api.MapGroup("/recipes").WithTags("Recipes");

        recipes.MapGet("/", ListRecipes.Handle).WithName("ListRecipes");

        return api;
    }
}
