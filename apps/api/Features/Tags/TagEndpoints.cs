namespace RecipEBook.Api.Features.Tags;

public static class TagEndpoints
{
    public static RouteGroupBuilder MapTagEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder tags = api.MapGroup("/tags").WithTags("Tags");

        tags.MapGet("/", ListTags.Handle).WithName("ListTags");

        return api;
    }
}
