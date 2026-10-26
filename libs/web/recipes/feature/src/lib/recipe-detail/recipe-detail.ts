import { Dialog } from '@angular/cdk/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import {
  TranslocoDatePipe,
  TranslocoDecimalPipe,
} from '@jsverse/transloco-locale';
import {
  RecipeApi,
  recipeDetailResource,
} from '@recip-e-book/web-recipes-data-access';
import { RecipeDetails } from '@recip-e-book/web-recipes-models';
import { Confirmation, Toaster } from '@recip-e-book/web-shared-ui';
import { DurationPipe } from '@recip-e-book/web-shared-util';
import {
  AddToShoppingList,
  AddToShoppingListData,
} from './add-to-shopping-list/add-to-shopping-list';

@Component({
  selector: 'lib-recipe-detail',
  imports: [
    RouterLink,
    TranslocoDirective,
    TranslocoDatePipe,
    TranslocoDecimalPipe,
    DurationPipe,
  ],
  templateUrl: './recipe-detail.html',
  styleUrl: './recipe-detail.css',
})
export class RecipeDetail {
  readonly id = input.required<string>();

  protected readonly recipe = recipeDetailResource(this.id);
  private readonly transloco = inject(TranslocoService);
  protected readonly lang = this.transloco.activeLang;

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  protected readonly deleting = signal(false);
  protected readonly deleteFailed = signal(false);

  private readonly api = inject(RecipeApi);
  private readonly confirmation = inject(Confirmation);
  private readonly dialog = inject(Dialog);
  private readonly router = inject(Router);
  private readonly toaster = inject(Toaster);

  protected addToShoppingList(recipe: RecipeDetails): void {
    this.dialog.open<void, AddToShoppingListData>(AddToShoppingList, {
      data: {
        recipeId: recipe.id,
        recipeTitle: recipe.title,
        servings: recipe.servings,
      },
      ariaLabelledBy: 'add-to-shopping-list-title',
    });
  }

  protected async delete(recipe: RecipeDetails): Promise<void> {
    const confirmed = await this.confirmation.ask({
      title: this.transloco.translate('recipes.detail.deleteTitle'),
      message: this.transloco.translate('recipes.detail.deleteMessage', {
        title: recipe.title,
      }),
      confirmLabel: this.transloco.translate('recipes.detail.delete'),
    });
    if (!confirmed) {
      return;
    }

    this.deleting.set(true);
    this.deleteFailed.set(false);
    try {
      await this.api.delete(recipe.id);
    } catch (error) {
      // A 404 means it is already gone: the outcome the user asked for.
      if (!(error instanceof HttpErrorResponse && error.status === 404)) {
        this.deleting.set(false);
        this.deleteFailed.set(true);
        return;
      }
    }
    this.toaster.show(this.transloco.translate('recipes.detail.deleted'));
    await this.router.navigate(['/recipes']);
  }
}
