import type { components } from '@recip-e-book/web-shared-api-types';

type Schemas = components['schemas'];

// Generated from the API's OpenAPI document (web-shared-api-types), named for the app.

// Write model: body of POST /api/recipes and PUT /api/recipes/{id}.
export type RecipeInput = Schemas['RecipeInput'];
export type IngredientInput = Schemas['IngredientInput'];

// Read models: responses of GET /api/recipes and GET /api/recipes/{id}.
export type RecipeSummary = Schemas['RecipeSummary'];
export type RecipeDetails = Schemas['RecipeDetails'];
export type IngredientDetails = Schemas['IngredientDetails'];
