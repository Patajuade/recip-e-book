namespace RecipEBook.Api.Domain;

/// <summary>
/// A tag of the fixed catalog (<see cref="TagCatalog"/>). <see cref="Key"/> is a stable
/// English identifier (`gluten-free`); the frontend translates it.
/// </summary>
public class Tag
{
    public required string Key { get; set; }
    public TagCategory Category { get; set; }
    public List<Recipe> Recipes { get; } = [];
}

public enum TagCategory
{
    Course,
    Diet,
    MainIngredient,
    CookingMethod,
    Season,
    Occasion,
    Cuisine,
    Practical,
}
