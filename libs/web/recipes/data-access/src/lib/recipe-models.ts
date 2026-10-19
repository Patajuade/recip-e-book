export interface RecipeSummary {
  id: string;
  title: string;
  totalTimeMinutes: number | null;
  tags: string[];
}

export interface IngredientDetails {
  name: string;
  quantity: number | null;
  unit: string | null;
}

export interface RecipeDetails {
  id: string;
  title: string;
  description: string | null;
  servings: number | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
  ingredients: IngredientDetails[];
  steps: string[];
  tags: string[];
}
