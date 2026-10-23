import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  signal,
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
import { FieldErrors, LabeledField } from '@recip-e-book/web-shared-ui';
import {
  emptyIngredient,
  emptyRecipe,
  emptyStep,
  recipeFormSchema,
  RecipeFormValue,
  toFormValue,
  toRecipeInput,
  toServerErrors,
} from './recipe-form';

@Component({
  selector: 'lib-recipe-edit',
  imports: [
    TranslocoDirective,
    FormField,
    FormRoot,
    RouterLink,
    LabeledField,
    FieldErrors,
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

  private readonly initialValue = (): RecipeFormValue =>
    this.recipe.hasValue() ? toFormValue(this.recipe.value()) : emptyRecipe();

  // The loaded recipe once it arrives, otherwise an empty one; editable either way.
  protected readonly model = linkedSignal<RecipeFormValue>(this.initialValue);

  private readonly saved = signal(false);

  // Compared as API bodies, so blank lines added and left empty don't count.
  readonly hasUnsavedChanges = computed(
    () =>
      !this.saved() &&
      JSON.stringify(toRecipeInput(this.model())) !==
        JSON.stringify(toRecipeInput(this.initialValue())),
  );

  private readonly api = inject(RecipeApi);
  private readonly router = inject(Router);

  constructor() {
    // Closing the tab or reloading: only the browser's own prompt is possible here.
    effect((onCleanup) => {
      if (!this.hasUnsavedChanges()) {
        return;
      }
      const warn = (event: BeforeUnloadEvent) => event.preventDefault();
      window.addEventListener('beforeunload', warn);
      onCleanup(() => window.removeEventListener('beforeunload', warn));
    });
  }

  protected readonly recipeForm = form(this.model, recipeFormSchema, {
    submission: {
      // Runs only when the form is valid. Returned errors go to their fieldTree,
      // or to the root field (shown as a banner) when they have none.
      action: async (recipeForm) => {
        const recipe = this.model();
        const id = this.id();
        try {
          const input = toRecipeInput(recipe);
          const savedId = id ?? (await this.api.create(input));
          if (id) {
            await this.api.update(id, input);
          }
          this.saved.set(true);
          await this.router.navigate(['/recipes', savedId]);
          return undefined;
        } catch (error) {
          if (
            error instanceof HttpErrorResponse &&
            error.status === 400 &&
            error.error?.errors
          ) {
            return toServerErrors(error.error.errors, recipeForm, recipe);
          }
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
