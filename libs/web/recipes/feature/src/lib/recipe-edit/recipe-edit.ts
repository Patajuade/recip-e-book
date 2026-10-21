import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, input, linkedSignal } from '@angular/core';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { recipeDetailResource } from '@recip-e-book/web-recipes-data-access';
import { FieldErrors } from '@recip-e-book/web-shared-ui';
import {
  emptyIngredient,
  emptyRecipe,
  emptyStep,
  recipeFormSchema,
  RecipeFormValue,
  toFormValue,
} from './recipe-form';

@Component({
  selector: 'lib-recipe-edit',
  imports: [TranslocoDirective, FormField, FormRoot, RouterLink, FieldErrors],
  templateUrl: './recipe-edit.html',
  styleUrl: './recipe-edit.css',
})
export class RecipeEdit {
  readonly id = input<string>();

  // Idle (no request) on /recipes/new, since there is no id.
  protected readonly recipe = recipeDetailResource(this.id);

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  // The loaded recipe once it arrives, otherwise an empty one; editable either way.
  protected readonly model = linkedSignal<RecipeFormValue>(() =>
    this.recipe.hasValue() ? toFormValue(this.recipe.value()) : emptyRecipe(),
  );

  protected readonly recipeForm = form(this.model, recipeFormSchema, {
    submission: {
      // Saving to the API comes with the RecipeApi service (#38).
      action: async () => undefined,
    },
  });

  protected addIngredient(): void {
    this.model.update((recipe) => ({
      ...recipe,
      ingredients: [...recipe.ingredients, emptyIngredient()],
    }));
  }

  protected removeIngredient(index: number): void {
    this.model.update((recipe) => ({
      ...recipe,
      ingredients: recipe.ingredients.filter((_, i) => i !== index),
    }));
  }

  protected addStep(): void {
    this.model.update((recipe) => ({
      ...recipe,
      steps: [...recipe.steps, emptyStep()],
    }));
  }

  protected removeStep(index: number): void {
    this.model.update((recipe) => ({
      ...recipe,
      steps: recipe.steps.filter((_, i) => i !== index),
    }));
  }
}
