namespace RecipEBook.Api.Features.ShoppingLists;

public static class ShoppingListEndpoints
{
    public static RouteGroupBuilder MapShoppingListEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder lists = api.MapGroup("/shopping-lists").WithTags("Shopping lists");

        lists.MapGet("/", ListShoppingLists.Handle).WithName("ListShoppingLists");
        lists.MapGet("/{id:guid}", GetShoppingList.Handle).WithName("GetShoppingList");

        return api;
    }
}
