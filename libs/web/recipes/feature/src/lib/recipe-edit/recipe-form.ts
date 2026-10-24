import {
  applyEach,
  FieldTree,
  max,
  maxLength,
  min,
  required,
  schema,
  ValidationError,
  WithOptionalFieldTree,
} from '@angular/forms/signals';
import { RecipeDetails, RecipeInput } from '@recip-e-book/web-recipes-models';

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

export const toFormValue = (recipe: RecipeDetails): RecipeFormValue => ({
  title: recipe.title,
  description: recipe.description ?? '',
  servings: recipe.servings,
  prepTimeMinutes: recipe.prepTimeMinutes,
  cookTimeMinutes: recipe.cookTimeMinutes,
  notes: recipe.notes ?? '',
  ingredients: recipe.ingredients.length
    ? recipe.ingredients.map((ingredient) => ({
        name: ingredient.name,
        quantity: ingredient.quantity,
        unit: ingredient.unit ?? '',
      }))
    : [emptyIngredient()],
  steps: recipe.steps.length
    ? recipe.steps.map((text) => ({ text }))
    : [emptyStep()],
  tags: recipe.tags.map((tag) => tag.key),
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

/** The API body: blank lines left out, empty texts sent as null. */
export const toRecipeInput = (recipe: RecipeFormValue): RecipeInput => ({
  title: recipe.title,
  description: recipe.description.trim() || null,
  servings: recipe.servings,
  prepTimeMinutes: recipe.prepTimeMinutes,
  cookTimeMinutes: recipe.cookTimeMinutes,
  notes: recipe.notes.trim() || null,
  ingredients: recipe.ingredients
    .filter((ingredient) => !isBlankIngredient(ingredient))
    .map((ingredient) => ({
      name: ingredient.name,
      quantity: ingredient.quantity,
      unit: ingredient.unit.trim() || null,
    })),
  steps: recipe.steps
    .filter((step) => !isBlankStep(step))
    .map((step) => step.text),
  tags: [...recipe.tags],
});

/**
 * Maps the API's 400 errors (ProblemDetails `errors`, keyed by C# property path such as
 * `Title` or `Ingredients[0].Name`) onto the form fields. Paths it doesn't know stay on the
 * root field. Indexes refer to the request, which has no blank lines.
 */
export function toServerErrors(
  errors: Record<string, string[]>,
  recipeForm: FieldTree<RecipeFormValue>,
  recipe: RecipeFormValue,
): WithOptionalFieldTree<ValidationError>[] {
  const ingredientIndexes = indexesWhere(
    recipe.ingredients,
    (ingredient) => !isBlankIngredient(ingredient),
  );
  const stepIndexes = indexesWhere(recipe.steps, (step) => !isBlankStep(step));
  const fields: Record<string, FieldTree<unknown>> = {
    Title: recipeForm.title,
    Description: recipeForm.description,
    Servings: recipeForm.servings,
    PrepTimeMinutes: recipeForm.prepTimeMinutes,
    CookTimeMinutes: recipeForm.cookTimeMinutes,
    Notes: recipeForm.notes,
  };

  const fieldFor = (path: string): FieldTree<unknown> | undefined => {
    const ingredient = /^Ingredients\[(\d+)\]\.(Name|Quantity|Unit)$/.exec(
      path,
    );
    if (ingredient) {
      const field = recipeForm.ingredients[ingredientIndexes[+ingredient[1]]];
      const property = ingredient[2].toLowerCase() as keyof IngredientFormValue;
      return field?.[property];
    }
    const step = /^Steps\[(\d+)\]$/.exec(path);
    if (step) {
      return recipeForm.steps[stepIndexes[+step[1]]]?.text;
    }
    if (/^Tags\[\d+\]$/.test(path)) {
      return recipeForm.tags;
    }
    return fields[path];
  };

  return Object.entries(errors).flatMap(([path, messages]) =>
    messages.map((message) => ({
      kind: 'server',
      message,
      fieldTree: fieldFor(path),
    })),
  );
}

const indexesWhere = <T>(items: T[], keep: (item: T) => boolean): number[] =>
  items.flatMap((item, index) => (keep(item) ? [index] : []));
