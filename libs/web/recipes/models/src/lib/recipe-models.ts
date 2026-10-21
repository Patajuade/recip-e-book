// Write model: body of POST /api/recipes and PUT /api/recipes/{id} (RecipeInput in the API).
export interface IngredientInput {
  name: string;
  quantity: number | null;
  unit: string | null;
}

export interface RecipeInput {
  title: string;
  description: string | null;
  servings: number | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  notes: string | null;
  ingredients: IngredientInput[];
  steps: string[];
  tags: string[];
}

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
