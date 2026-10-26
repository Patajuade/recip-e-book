namespace RecipEBook.Api.Features.ShoppingLists;

public static class ShoppingListEndpoints
{
    public static RouteGroupBuilder MapShoppingListEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder lists = api.MapGroup("/shopping-lists").WithTags("Shopping lists");

        lists.MapGet("/", ListShoppingLists.Handle).WithName("ListShoppingLists");
        lists.MapGet("/{id:guid}", GetShoppingList.Handle).WithName("GetShoppingList");
        lists.MapPost("/", CreateShoppingList.Handle).WithName("CreateShoppingList").ProducesValidationProblem();
        lists.MapPut("/{id:guid}", RenameShoppingList.Handle).WithName("RenameShoppingList").ProducesValidationProblem();
        lists.MapDelete("/{id:guid}", DeleteShoppingList.Handle).WithName("DeleteShoppingList");

        lists.MapPost("/{id:guid}/items", AddShoppingListItem.Handle)
            .WithName("AddShoppingListItem")
            .ProducesValidationProblem();
        lists.MapPut("/{id:guid}/items/{itemId:guid}", UpdateShoppingListItem.Handle)
            .WithName("UpdateShoppingListItem")
            .ProducesValidationProblem();
        lists.MapDelete("/{id:guid}/items/{itemId:guid}", DeleteShoppingListItem.Handle)
            .WithName("DeleteShoppingListItem");
        lists.MapDelete("/{id:guid}/checked-items", ClearCheckedItems.Handle).WithName("ClearCheckedItems");

        return api;
    }
}
