import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { form } from '@angular/forms/signals';
import { RecipeDetails } from '@recip-e-book/web-recipes-models';
import {
  emptyIngredient,
  emptyRecipe,
  emptyStep,
  RecipeFormValue,
  recipeFormSchema,
  toFormValue,
  toRecipeInput,
  toServerErrors,
} from './recipe-form';

const recipe: RecipeDetails = {
  id: '1',
  title: 'Soupe',
  description: null,
  servings: 4,
  prepTimeMinutes: 10,
  cookTimeMinutes: null,
  notes: null,
  ingredients: [{ name: 'carottes', quantity: 3, unit: null }],
  steps: ['Couper.'],
  tags: [{ key: 'soup', category: 'course' }],
  createdAt: '2026-10-01T10:00:00Z',
  updatedAt: '2026-10-02T10:00:00Z',
};

describe('recipe form', () => {
  it('turns a recipe into form values and back', () => {
    const value = toFormValue(recipe);

    expect(value.description).toBe('');
    expect(value.ingredients).toEqual([
      { name: 'carottes', quantity: 3, unit: '' },
    ]);
    expect(value.tags).toEqual(['soup']);
    expect(toRecipeInput(value)).toEqual({
      title: 'Soupe',
      description: null,
      servings: 4,
      prepTimeMinutes: 10,
      cookTimeMinutes: null,
      notes: null,
      ingredients: [{ name: 'carottes', quantity: 3, unit: null }],
      steps: ['Couper.'],
      tags: ['soup'],
    });
  });

  it('gives an empty line to a recipe without ingredients or steps', () => {
    const value = toFormValue({ ...recipe, ingredients: [], steps: [] });

    expect(value.ingredients).toEqual([emptyIngredient()]);
    expect(value.steps).toEqual([emptyStep()]);
  });

  it('leaves blank lines out of the API body and sends empty texts as null', () => {
    const value: RecipeFormValue = {
      ...emptyRecipe(),
      title: 'Soupe',
      description: '   ',
      ingredients: [
        emptyIngredient(),
        { name: 'sel', quantity: null, unit: '  ' },
      ],
      steps: [{ text: ' ' }, { text: 'Saler.' }],
    };

    const input = toRecipeInput(value);

    expect(input.description).toBeNull();
    expect(input.ingredients).toEqual([
      { name: 'sel', quantity: null, unit: null },
    ]);
    expect(input.steps).toEqual(['Saler.']);
  });

  it('puts API errors under the fields they are about, blank lines skipped', () => {
    const value: RecipeFormValue = {
      ...emptyRecipe(),
      title: 'Soupe',
      // Line 0 is blank, so the API's Ingredients[0] is the form's line 1.
      ingredients: [emptyIngredient(), { name: 'x', quantity: 1, unit: '' }],
      steps: [{ text: 'Couper.' }],
    };
    const recipeForm = TestBed.runInInjectionContext(() =>
      form(signal(value), recipeFormSchema),
    );

    const errors = toServerErrors(
      {
        Title: ['Too long.'],
        'Ingredients[0].Name': ['Too short.'],
        'Steps[0]': ['Too long.'],
        'Tags[2]': ["Unknown tag 'x'."],
        Something: ['Other.'],
      },
      recipeForm,
      value,
    );

    expect(errors.map((error) => [error.message, error.fieldTree])).toEqual([
      ['Too long.', recipeForm.title],
      ['Too short.', recipeForm.ingredients[1].name],
      ['Too long.', recipeForm.steps[0].text],
      ["Unknown tag 'x'.", recipeForm.tags],
      ['Other.', undefined],
    ]);
  });

  it('requires an ingredient name only when the line has a quantity or a unit', () => {
    const value = signal<RecipeFormValue>({
      ...emptyRecipe(),
      title: 'Soupe',
      ingredients: [emptyIngredient(), { name: '', quantity: 2, unit: '' }],
    });
    const recipeForm = TestBed.runInInjectionContext(() =>
      form(value, recipeFormSchema),
    );

    expect(recipeForm.ingredients[0].name().invalid()).toBe(false);
    expect(recipeForm.ingredients[1].name().invalid()).toBe(true);
  });
});
