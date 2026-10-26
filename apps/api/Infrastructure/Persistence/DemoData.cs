using Microsoft.EntityFrameworkCore;

using RecipEBook.Api.Domain;

namespace RecipEBook.Api.Infrastructure.Persistence;

/// <summary>
/// Demo recipes and shopping list for local development, registered with
/// UseSeeding/UseAsyncSeeding. Seeding runs after every migration/database update, so each
/// kind of data is only inserted into a database that has none yet.
/// </summary>
internal static class DemoData
{
    private const string SoupTitle = "Soupe de légumes improvisée";

    public static void Seed(DbContext context)
    {
        if (!context.Set<Recipe>().Any())
        {
            Dictionary<string, Tag> tags = context.Set<Tag>().ToDictionary(t => t.Key);
            context.Set<Recipe>().AddRange(CreateRecipes(tags));
            context.SaveChanges();
        }

        if (!context.Set<ShoppingList>().Any())
        {
            Recipe? soup = context.Set<Recipe>().FirstOrDefault(r => r.Title == SoupTitle);
            context.Set<ShoppingList>().Add(CreateShoppingList(soup));
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Set<Recipe>().AnyAsync(cancellationToken))
        {
            Dictionary<string, Tag> tags = await context.Set<Tag>().ToDictionaryAsync(t => t.Key, cancellationToken);
            context.Set<Recipe>().AddRange(CreateRecipes(tags));
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Set<ShoppingList>().AnyAsync(cancellationToken))
        {
            Recipe? soup = await context.Set<Recipe>().FirstOrDefaultAsync(r => r.Title == SoupTitle, cancellationToken);
            context.Set<ShoppingList>().Add(CreateShoppingList(soup));
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    // Some items come from the soup (when that demo recipe exists), the others are free items.
    private static ShoppingList CreateShoppingList(Recipe? soup)
    {
        DateTime now = DateTime.UtcNow;
        ShoppingList list = new() { Name = "Courses de la semaine", CreatedAt = now, UpdatedAt = now };
        (string Name, decimal? Quantity, string? Unit, bool FromSoup, bool Checked)[] items =
        [
            ("carottes", 3, "pièce", true, false),
            ("poireau", 1, "pièce", true, false),
            ("pommes de terre", 2, "pièce", true, true),
            ("lait", 1, "l", false, false),
            ("pain", null, null, false, true),
            ("café", 250, "g", false, false),
        ];
        list.Items.AddRange(items.Select((item, position) => new ShoppingListItem
        {
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Checked = item.Checked,
            Position = position,
            SourceRecipe = item.FromSoup ? soup : null,
        }));
        return list;
    }

    // Tags come from the catalog, inserted by migrations before seeding runs.
    private static List<Recipe> CreateRecipes(Dictionary<string, Tag> tags)
    {
        DateTime now = DateTime.UtcNow;


        return
        [
            new Recipe
            {
                Title = SoupTitle,
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
                Tags = { tags["soup"], tags["vegetarian"], tags["winter"], tags["vegetables"], tags["stovetop"] },
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
                Tags = { tags["main-course"], tags["vegetarian"], tags["pasta"], tags["quick"], tags["italian"] },
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
                Tags = { tags["dessert"], tags["vegetarian"], tags["fruit"], tags["oven"], tags["autumn"] },
            },
        ];
    }
}
