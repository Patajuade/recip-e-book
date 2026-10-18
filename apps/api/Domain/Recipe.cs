namespace RecipEBook.Api.Domain;

public class Recipe
{
    private string _title = "";

    public Guid Id { get; set; }

    public required string Title
    {
        get => _title;
        set
        {
            _title = value;
            SearchTitle = SearchText.Normalize(value);
        }
    }

    /// <summary>
    /// Lowercase, accent-free copy of <see cref="Title"/> used for searching.
    /// Kept in sync by the Title setter.
    /// </summary>
    public string SearchTitle { get; private set; } = "";

    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Ingredient> Ingredients { get; set; } = [];
    public List<RecipeStep> Steps { get; set; } = [];
    public List<Tag> Tags { get; } = [];
}