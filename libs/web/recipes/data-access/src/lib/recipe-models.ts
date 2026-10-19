// Read models returned by the API (camelCase JSON). Keep them in sync with the
// C# DTOs they mirror. Nullable C# values are always present, possibly `null`.

/** Item of `GET /api/recipes` (`ListRecipes.RecipeSummary`). */
export interface RecipeSummary {
  id: string;
  title: string;
  totalTimeMinutes: number | null;
  tags: string[];
}

/** Ingredient of a recipe (`GetRecipe.IngredientDetails`). */
export interface IngredientDetails {
  name: string;
  quantity: number | null;
  unit: string | null;
}

/** Response of `GET /api/recipes/{id}` (`GetRecipe.RecipeDetails`). */
export interface RecipeDetails {
  id: string;
  title: string;
  description: string | null;
  servings: number | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  notes: string | null;
  /** ISO 8601 date in UTC (ends with `Z`). */
  createdAt: string;
  /** ISO 8601 date in UTC (ends with `Z`). */
  updatedAt: string;
  ingredients: IngredientDetails[];
  steps: string[];
  tags: string[];
}
