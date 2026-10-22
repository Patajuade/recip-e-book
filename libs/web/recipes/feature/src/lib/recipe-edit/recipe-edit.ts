import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  inject,
  input,
  linkedSignal,
} from '@angular/core';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import {
  RecipeApi,
  recipeDetailResource,
  tagCatalogResource,
} from '@recip-e-book/web-recipes-data-access';
import { TagPicker } from '@recip-e-book/web-recipes-ui';
import { LabeledField } from '@recip-e-book/web-shared-ui';
import {
  emptyIngredient,
  emptyRecipe,
  emptyStep,
  recipeFormSchema,
  RecipeFormValue,
  toFormValue,
  toRecipeInput,
} from './recipe-form';

@Component({
  selector: 'lib-recipe-edit',
  imports: [
    TranslocoDirective,
    FormField,
    FormRoot,
    RouterLink,
    LabeledField,
    TagPicker,
  ],
  templateUrl: './recipe-edit.html',
  styleUrl: './recipe-edit.css',
})
export class RecipeEdit {
  readonly id = input<string>();

  // Idle (no request) on /recipes/new, since there is no id.
  protected readonly recipe = recipeDetailResource(this.id);

  protected readonly tagCatalog = tagCatalogResource();

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  // The loaded recipe once it arrives, otherwise an empty one; editable either way.
  protected readonly model = linkedSignal<RecipeFormValue>(() =>
    this.recipe.hasValue() ? toFormValue(this.recipe.value()) : emptyRecipe(),
  );

  private readonly api = inject(RecipeApi);
  private readonly router = inject(Router);

  protected readonly recipeForm = form(this.model, recipeFormSchema, {
    submission: {
      // Runs only when the form is valid; a returned error is set on the root field.
      action: async () => {
        const input = toRecipeInput(this.model());
        const id = this.id();
        try {
          const savedId = id ?? (await this.api.create(input));
          if (id) {
            await this.api.update(id, input);
          }
          await this.router.navigate(['/recipes', savedId]);
          return undefined;
        } catch {
          return { kind: 'saveFailed' };
        }
      },
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
