import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import {
  TranslocoDatePipe,
  TranslocoDecimalPipe,
} from '@jsverse/transloco-locale';
import { recipeDetailResource } from '@recip-e-book/web-recipes-data-access';

@Component({
  selector: 'lib-recipe-detail',
  imports: [
    RouterLink,
    TranslocoDirective,
    TranslocoDatePipe,
    TranslocoDecimalPipe,
  ],
  templateUrl: './recipe-detail.html',
  styleUrl: './recipe-detail.css',
})
export class RecipeDetail {
  readonly id = input.required<string>();

  protected readonly recipe = recipeDetailResource(this.id);

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });
}
