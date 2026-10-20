using System.Net;

namespace RecipEBook.Api.IntegrationTests;

public class HealthCheckTests(RecipeBookApiFactory factory) : IClassFixture<RecipeBookApiFactory>
{
    [Fact]
    public async Task Get_Health_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
