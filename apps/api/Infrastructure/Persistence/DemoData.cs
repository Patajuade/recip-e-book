using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence;

/// <summary>
/// Demo recipes for local development, registered with UseSeeding/UseAsyncSeeding.
/// Seeding runs after every migration/database update, so it only inserts data
/// into a database that has no recipe yet.
/// </summary>
internal static class DemoData
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Recipe>().Any())
        {
            return;
        }

        context.Set<Recipe>().AddRange(CreateRecipes());
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (await context.Set<Recipe>().AnyAsync(cancellationToken))
        {
            return;
        }

        context.Set<Recipe>().AddRange(CreateRecipes());
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<Recipe> CreateRecipes()
    {
        DateTime now = DateTime.UtcNow;

        Tag soup = new() { Name = "soupe" };
        Tag vegetarian = new() { Name = "végétarien" };
        Tag winter = new() { Name = "hiver" };
        Tag quick = new() { Name = "rapide" };
        Tag dessert = new() { Name = "dessert" };

        return
        [
            new Recipe
            {
                Title = "Soupe de légumes improvisée",
                Description = "Une soupe faite un peu au pif avec ce qui restait dans le frigo.",
                Servings = 4,
                PrepTimeMinutes = 15,
                CookTimeMinutes = 30,
                Notes = "La prochaine fois, essayer avec une pointe de cumin.",
                CreatedAt = now,
                UpdatedAt = now,
                Ingredients =
                [
                    new Ingredient { Name = "carottes", Quantity = 3, Unit = "pièce" },
                    new Ingredient { Name = "poireau", Quantity = 1, Unit = "pièce" },
                    new Ingredient { Name = "pommes de terre", Quantity = 2, Unit = "pièce" },
                    new Ingredient { Name = "bouillon de légumes", Quantity = 1, Unit = "l" },
                    new Ingredient { Name = "sel et poivre" },
                ],
                Steps =
                [
                    new RecipeStep { Instruction = "Éplucher et couper les légumes en morceaux." },
                    new RecipeStep { Instruction = "Les faire revenir 5 minutes dans une casserole avec un filet d'huile." },
                    new RecipeStep { Instruction = "Ajouter le bouillon et laisser cuire 25 minutes à feu doux." },
                    new RecipeStep { Instruction = "Mixer, puis saler et poivrer." },
                ],
                Tags = { soup, vegetarian, winter },
            },
            new Recipe
            {
                Title = "Pâtes à l'ail et au parmesan",
                Servings = 2,
                PrepTimeMinutes = 5,
                CookTimeMinutes = 10,
                CreatedAt = now,
                UpdatedAt = now,
                Ingredients =
                [
                    new Ingredient { Name = "spaghettis", Quantity = 200, Unit = "g" },
                    new Ingredient { Name = "ail", Quantity = 2, Unit = "gousse" },
                    new Ingredient { Name = "parmesan râpé", Quantity = 50, Unit = "g" },
                    new Ingredient { Name = "huile d'olive", Quantity = 2, Unit = "c. à s." },
                ],
                Steps =
                [
                    new RecipeStep { Instruction = "Cuire les pâtes dans une grande casserole d'eau salée." },
                    new RecipeStep { Instruction = "Faire dorer l'ail émincé dans l'huile d'olive." },
                    new RecipeStep { Instruction = "Mélanger les pâtes égouttées avec l'ail et le parmesan." },
                ],
                Tags = { vegetarian, quick },
            },
            new Recipe
            {
                Title = "Crumble aux pommes",
                Description = "Le dessert de dimanche soir.",
                Servings = 6,
                PrepTimeMinutes = 20,
                CookTimeMinutes = 35,
                CreatedAt = now,
                UpdatedAt = now,
                Ingredients =
                [
                    new Ingredient { Name = "pommes", Quantity = 6, Unit = "pièce" },
                    new Ingredient { Name = "farine", Quantity = 150, Unit = "g" },
                    new Ingredient { Name = "beurre", Quantity = 100, Unit = "g" },
                    new Ingredient { Name = "sucre", Quantity = 100, Unit = "g" },
                    new Ingredient { Name = "cannelle", Quantity = 0.5m, Unit = "c. à c." },
                ],
                Steps =
                [
                    new RecipeStep { Instruction = "Préchauffer le four à 180 °C." },
                    new RecipeStep { Instruction = "Couper les pommes en dés et les répartir dans un plat avec la cannelle." },
                    new RecipeStep { Instruction = "Sabler la farine, le beurre et le sucre du bout des doigts, puis recouvrir les pommes." },
                    new RecipeStep { Instruction = "Enfourner pour 35 minutes." },
                ],
                Tags = { dessert, vegetarian },
            },
        ];
    }
}
