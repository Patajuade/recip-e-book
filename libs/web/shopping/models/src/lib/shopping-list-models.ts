import type { components } from '@recip-e-book/web-shared-api-types';

type Schemas = components['schemas'];

// Generated from the API's OpenAPI document (web-shared-api-types), named for the app.

// Read models: responses of GET /api/shopping-lists and GET /api/shopping-lists/{id}.
export type ShoppingListSummary = Schemas['ShoppingListSummary'];
export type ShoppingListDetails = Schemas['ShoppingListDetails'];
export type ShoppingListItem = Schemas['ItemDetails'];
export type SourceRecipe = Schemas['SourceRecipe'];

// Write models: request bodies (ShoppingListInput, ShoppingListItemInput, AddRecipeInput in the API).
export type ShoppingListInput = Schemas['ShoppingListInput'];
export type ShoppingListItemInput = Schemas['ShoppingListItemInput'];
export type AddRecipeInput = Schemas['AddRecipeInput'];
