namespace RecipEBook.Api.Domain;

/// <summary>
/// The tags users can pick from. Inserted by migrations (model managed data): adding,
/// removing or renaming a tag needs a new migration.
/// </summary>
public static class TagCatalog
{
    public static IReadOnlyList<Tag> Tags { get; } =
    [
        .. Category(TagCategory.Course,
            "breakfast", "brunch", "appetizer", "starter", "main-course", "side-dish", "soup",
            "salad", "dessert", "snack", "drink", "sauce", "bread"),
        .. Category(TagCategory.Diet,
            "vegetarian", "vegan", "pescatarian", "gluten-free", "dairy-free", "lactose-free",
            "egg-free", "nut-free", "low-carb", "low-fat", "low-salt", "low-calorie",
            "high-protein", "sugar-free", "halal", "kosher"),
        .. Category(TagCategory.MainIngredient,
            "beef", "veal", "pork", "lamb", "poultry", "fish", "seafood", "eggs", "cheese",
            "pasta", "rice", "potatoes", "legumes", "vegetables", "mushrooms", "fruit",
            "chocolate", "grains"),
        .. Category(TagCategory.CookingMethod,
            "oven", "stovetop", "fried", "grilled", "barbecue", "steamed", "slow-cooked",
            "pressure-cooker", "air-fryer", "microwave", "no-cook", "one-pot", "raw"),
        .. Category(TagCategory.Season,
            "spring", "summer", "autumn", "winter"),
        .. Category(TagCategory.Occasion,
            "everyday", "party", "christmas", "easter", "new-year", "birthday", "picnic",
            "lunchbox", "batch-cooking", "kids", "date-night", "brunch-party", "comfort-food",
            "buffet"),
        .. Category(TagCategory.Cuisine,
            "french", "italian", "spanish", "portuguese", "greek", "mediterranean",
            "middle-eastern", "north-african", "african", "indian", "chinese", "japanese",
            "korean", "thai", "vietnamese", "mexican", "american", "caribbean"),
        .. Category(TagCategory.Practical,
            "quick", "easy", "budget", "make-ahead", "freezer-friendly"),
    ];

    private static readonly Dictionary<string, int> Positions = Tags
        .Select((tag, index) => (tag.Key, index))
        .ToDictionary(tag => tag.Key, tag => tag.index);

    /// <summary>
    /// A tag's place in the catalog, which is grouped by category: sorting by it groups tags
    /// by category (dish type first). The database can't sort that way, it stores the category
    /// as text.
    /// </summary>
    public static int Position(string key) => Positions.GetValueOrDefault(key, int.MaxValue);

    private static IEnumerable<Tag> Category(TagCategory category, params string[] keys) =>
        keys.Select(key => new Tag { Key = key, Category = category });
}
