import { httpResource } from '@angular/common/http';
import { RecipeDetails, RecipeSummary } from '@recip-e-book/web-recipes-models';

export interface RecipeListFilters {
  search?: string;
  tag?: string;
}

export function recipeListResource(
  filters: () => RecipeListFilters = () => ({}),
) {
  return httpResource<RecipeSummary[]>(() => {
    const { search, tag } = filters();
    const params: Record<string, string> = {};
    if (search) {
      params['search'] = search;
    }
    if (tag) {
      params['tag'] = tag;
    }
    return { url: '/api/recipes', params };
  });
}

export function recipeDetailResource(id: () => string | undefined) {
  return httpResource<RecipeDetails>(() => {
    const recipeId = id();
    return recipeId
      ? `/api/recipes/${encodeURIComponent(recipeId)}`
      : undefined;
  });
}
