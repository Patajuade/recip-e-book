namespace RecipEBook.Api.Features.Accounts;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder api)
    {
        RouteGroupBuilder auth = api.MapGroup("/auth").WithTags("Accounts");

        auth.MapGet("/session", GetSession.Handle).WithName("GetSession");
        auth.MapPost("/register", Register.Handle).WithName("Register").ProducesValidationProblem();
        auth.MapPost("/login", Login.Handle).WithName("Login").ProducesValidationProblem();
        auth.MapPost("/logout", Logout.Handle).WithName("Logout").RequireAuthorization();

        return api;
    }
}
