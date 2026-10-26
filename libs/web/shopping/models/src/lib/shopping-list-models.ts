// Read models: responses of GET /api/shopping-lists and GET /api/shopping-lists/{id}.
export interface ShoppingListSummary {
  id: string;
  name: string;
  itemCount: number;
  checkedCount: number;
  updatedAt: string;
}

export interface SourceRecipe {
  id: string;
  title: string;
}

export interface ShoppingListItem {
  id: string;
  name: string;
  quantity: number | null;
  unit: string | null;
  checked: boolean;
  position: number;
  sourceRecipe: SourceRecipe | null;
}

export interface ShoppingListDetails {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  items: ShoppingListItem[];
}

// Write models: request bodies (ShoppingListInput, ShoppingListItemInput, AddRecipeInput in the API).
export interface ShoppingListInput {
  name: string;
}

export interface ShoppingListItemInput {
  name: string;
  quantity: number | null;
  unit: string | null;
  checked: boolean;
}

export interface AddRecipeInput {
  recipeId: string;
  servings: number | null;
}
