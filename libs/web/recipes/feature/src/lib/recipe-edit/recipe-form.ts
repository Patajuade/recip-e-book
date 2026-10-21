import {
  applyEach,
  max,
  maxLength,
  min,
  required,
  schema,
} from '@angular/forms/signals';

export interface IngredientFormValue {
  name: string;
  quantity: number | null;
  unit: string;
}

export interface StepFormValue {
  text: string;
}

export interface RecipeFormValue {
  title: string;
  description: string;
  servings: number | null;
  prepTimeMinutes: number | null;
  cookTimeMinutes: number | null;
  notes: string;
  ingredients: IngredientFormValue[];
  steps: StepFormValue[];
  tags: string[];
}

export const emptyIngredient = (): IngredientFormValue => ({
  name: '',
  quantity: null,
  unit: '',
});

export const emptyStep = (): StepFormValue => ({ text: '' });

export const emptyRecipe = (): RecipeFormValue => ({
  title: '',
  description: '',
  servings: null,
  prepTimeMinutes: null,
  cookTimeMinutes: null,
  notes: '',
  ingredients: [emptyIngredient()],
  steps: [emptyStep()],
  tags: [],
});

export const recipeFormSchema = schema<RecipeFormValue>((recipe) => {
  required(recipe.title);
  maxLength(recipe.title, 200);
  maxLength(recipe.description, 2000);
  min(recipe.servings, 1);
  max(recipe.servings, 100);
  min(recipe.prepTimeMinutes, 0);
  max(recipe.prepTimeMinutes, 1440);
  min(recipe.cookTimeMinutes, 0);
  max(recipe.cookTimeMinutes, 1440);
  maxLength(recipe.notes, 4000);

  applyEach(recipe.ingredients, (ingredient) => {
    required(ingredient.name, {
      when: ({ valueOf }) =>
        valueOf(ingredient.quantity) !== null ||
        valueOf(ingredient.unit).trim() !== '',
    });
    maxLength(ingredient.name, 200);
    min(ingredient.quantity, 0);
    max(ingredient.quantity, 100_000);
    maxLength(ingredient.unit, 50);
  });

  applyEach(recipe.steps, (step) => {
    maxLength(step.text, 2000);
  });
});

// Blank lines are not errors: they are left out of what is sent to the API.
export const isBlankIngredient = (ingredient: IngredientFormValue): boolean =>
  ingredient.name.trim() === '' &&
  ingredient.quantity === null &&
  ingredient.unit.trim() === '';

export const isBlankStep = (step: StepFormValue): boolean =>
  step.text.trim() === '';
