import { Component, input, signal } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { TranslocoDirective } from '@jsverse/transloco';
import {
  emptyIngredient,
  emptyRecipe,
  emptyStep,
  recipeFormSchema,
  RecipeFormValue,
} from './recipe-form';
import { JsonPipe } from '@angular/common';
import { FieldErrors } from '@recip-e-book/web-shared-ui';

@Component({
  selector: 'lib-recipe-edit',
  imports: [TranslocoDirective, FormField, JsonPipe, FieldErrors],
  templateUrl: './recipe-edit.html',
  styleUrl: './recipe-edit.css',
})
export class RecipeEdit {
  readonly id = input<string>();

  protected readonly model = signal<RecipeFormValue>(emptyRecipe());
  protected readonly recipeForm = form(this.model, recipeFormSchema);
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
